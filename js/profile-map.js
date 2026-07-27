/**
 * Profile map — public missing-persons map by default; user's runner(s) when registered with coordinates.
 */
(function () {
  'use strict';

  const HOUSTON_LAT = 29.7604;
  const HOUSTON_LNG = -95.3698;

  let map = null;
  let infoWindow = null;
  let markers = [];
  let initialized = false;
  let mapMode = 'public';

  function getApiBase() {
    return window.APP_CONFIG?.API_BASE_URL || 'https://two41runners-api.onrender.com/api';
  }

  function getApiKey() {
    const config = window.GOOGLE_MAPS_CONFIG || {};
    return config.API_KEY || window.APP_CONFIG?.GOOGLE_MAPS_API_KEY || '';
  }

  function getAuthToken() {
    try {
      const authData = localStorage.getItem('ra_auth');
      if (authData) {
        const auth = JSON.parse(authData);
        return auth.accessToken || auth.token || null;
      }
    } catch (e) {
      /* ignore */
    }
    return null;
  }

  function getStatusColor(status) {
    const colors = (window.GOOGLE_MAPS_CONFIG || {}).STATUS_COLORS || {};
    return colors[(status || '').toLowerCase()] || '#3b82f6';
  }

  function setDescription(text) {
    const el = document.getElementById('profileMapDescription');
    if (el) el.textContent = text;
  }

  function setLoading(show) {
    const el = document.getElementById('profileMapLoading');
    if (el) el.style.display = show ? 'block' : 'none';
  }

  function normalizePublicItem(item) {
    const normalized = window.publicMapApi?.normalizeMapItem(item) || item;
    return {
      id: normalized.id,
      displayName: normalized.displayName,
      latitude: Number(normalized.latitude),
      longitude: Number(normalized.longitude),
      currentStatus: normalized.status || 'Missing',
      lastSeenCityState: normalized.lastSeenCityState || 'Unknown',
      lastSeenDate: normalized.updatedAt,
    };
  }

  function normalizeRunner(runner) {
    return {
      id: runner.id,
      displayName: runner.name || `${runner.firstName || ''} ${runner.lastName || ''}`.trim() || `Runner #${runner.id}`,
      latitude: Number(runner.mapLatitude),
      longitude: Number(runner.mapLongitude),
      currentStatus: runner.status || 'Active',
      lastSeenCityState: runner.lastKnownLocation || 'Your runner',
      lastSeenDate: runner.updatedAt,
      isOwnRunner: true,
    };
  }

  async function fetchUserRunners(token) {
    const res = await fetch(`${getApiBase()}/v1/runner?page=1&pageSize=50`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) return [];
    const data = await res.json();
    return Array.isArray(data.runners) ? data.runners : [];
  }

  async function fetchPublicPoints() {
    const api = window.publicMapApi;
    if (!api) throw new Error('Map API unavailable');
    const items = await api.getMissingMapCases();
    return items
      .map(normalizePublicItem)
      .filter(p => Number.isFinite(p.latitude) && Number.isFinite(p.longitude));
  }

  async function resolveMapData() {
    const token = getAuthToken();
    if (token) {
      try {
        const runners = await fetchUserRunners(token);
        const withCoords = runners.filter(
          r => r.mapLatitude != null && r.mapLongitude != null
        );
        if (withCoords.length > 0) {
          mapMode = 'mine';
          return {
            points: withCoords.map(normalizeRunner),
            description: `Showing ${withCoords.length} registered runner${withCoords.length > 1 ? 's' : ''} on the map.`,
          };
        }
        if (runners.length > 0) {
          mapMode = 'public';
          return {
            points: await fetchPublicPoints(),
            description:
              'Your runner profile does not have map coordinates yet. Showing community missing-persons map.',
          };
        }
      } catch (err) {
        console.warn('Profile map: could not load user runners', err);
      }
    }

    mapMode = 'public';
    return {
      points: await fetchPublicPoints(),
      description: 'Showing missing-persons cases in the community.',
    };
  }

  function clearMarkers() {
    markers.forEach(m => m.setMap(null));
    markers = [];
  }

  function fitMapToPoints(points) {
    if (!map || !points.length) return;
    if (points.length === 1) {
      map.setCenter({ lat: points[0].latitude, lng: points[0].longitude });
      map.setZoom(14);
      return;
    }
    const bounds = new google.maps.LatLngBounds();
    points.forEach(p => bounds.extend({ lat: p.latitude, lng: p.longitude }));
    map.fitBounds(bounds, 48);
  }

  function renderMarkers(points) {
    clearMarkers();
    points.forEach(point => {
      const marker = new google.maps.Marker({
        position: { lat: point.latitude, lng: point.longitude },
        map,
        title: point.displayName,
        icon: {
          path: google.maps.SymbolPath.CIRCLE,
          scale: point.isOwnRunner ? 12 : 10,
          fillColor: point.isOwnRunner ? '#2563eb' : getStatusColor(point.currentStatus),
          fillOpacity: 1,
          strokeColor: '#ffffff',
          strokeWeight: 2,
        },
      });
      marker.addListener('click', () => {
        const statusLabel = point.isOwnRunner ? 'Your runner' : point.currentStatus;
        infoWindow.setContent(`
          <div style="padding:10px;max-width:240px;font-family:Arial,sans-serif;">
            <h3 style="margin:0 0 8px;color:#dc2626;font-size:1rem;">${point.displayName}</h3>
            <p style="margin:4px 0;font-size:13px;color:#555;"><strong>Status:</strong> ${statusLabel}</p>
            <p style="margin:4px 0;font-size:13px;color:#555;"><strong>Location:</strong> ${point.lastSeenCityState}</p>
          </div>`);
        infoWindow.open(map, marker);
      });
      markers.push(marker);
    });
    fitMapToPoints(points);
  }

  async function loadMapData() {
    setLoading(true);
    try {
      const { points, description } = await resolveMapData();
      setDescription(description);
      renderMarkers(points);
      const emptyEl = document.getElementById('profileMapEmpty');
      if (emptyEl) emptyEl.style.display = points.length ? 'none' : 'block';
    } catch (err) {
      console.error('Profile map load failed:', err);
      setDescription('Unable to load map data. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  function showMapError() {
    const container = document.getElementById('profileMap');
    if (!container) return;
    container.innerHTML =
      '<div style="display:flex;align-items:center;justify-content:center;height:100%;background:#f3f4f6;border-radius:12px;padding:24px;text-align:center;color:#666;">Google Maps API key required. Set GOOGLE_MAPS_API_KEY in config.json.</div>';
  }

  function initProfileMapInstance() {
    const apiKey = getApiKey();
    if (!apiKey || apiKey === 'YOUR_GOOGLE_MAPS_API_KEY') {
      showMapError();
      return;
    }

    const container = document.getElementById('profileMap');
    if (!container || typeof google === 'undefined' || !google.maps) return;

    const config = window.GOOGLE_MAPS_CONFIG || {};
    map = new google.maps.Map(container, {
      center: {
        lat: config.DEFAULT_CENTER?.lat || HOUSTON_LAT,
        lng: config.DEFAULT_CENTER?.lng || HOUSTON_LNG,
      },
      zoom: config.DEFAULT_ZOOM || 10,
      styles: config.MAP_STYLES || [],
      mapTypeId: google.maps.MapTypeId.ROADMAP,
    });
    infoWindow = new google.maps.InfoWindow();
    initialized = true;
    loadMapData();
  }

  function loadGoogleMaps(callback) {
    const apiKey = getApiKey();
    if (!apiKey || apiKey === 'YOUR_GOOGLE_MAPS_API_KEY') {
      showMapError();
      return;
    }
    if (typeof google !== 'undefined' && google.maps) {
      callback();
      return;
    }
    window.initProfileGoogleMap = callback;
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&callback=initProfileGoogleMap&loading=async`;
    script.async = true;
    script.defer = true;
    script.onerror = showMapError;
    document.head.appendChild(script);
  }

  function toggleMap() {
    const wrapper = document.getElementById('profileMapWrapper');
    const btn = document.getElementById('toggleProfileMapBtn');
    if (!wrapper) return;

    const isOpen = wrapper.classList.toggle('is-open');
    if (btn) {
      btn.textContent = isOpen ? 'Hide Map' : 'View Map';
      btn.setAttribute('aria-expanded', String(isOpen));
    }

    if (isOpen && !initialized) {
      loadGoogleMaps(initProfileMapInstance);
    } else if (isOpen && initialized) {
      loadMapData();
    }
  }

  function bindUi() {
    const btn = document.getElementById('toggleProfileMapBtn');
    const refreshBtn = document.getElementById('refreshProfileMapBtn');
    if (btn) btn.addEventListener('click', toggleMap);
    if (refreshBtn) {
      refreshBtn.addEventListener('click', () => {
        if (initialized) loadMapData();
      });
    }
  }

  document.addEventListener('DOMContentLoaded', bindUi);

  window.profileMap = {
    toggle: toggleMap,
    refresh: () => initialized && loadMapData(),
  };
})();
