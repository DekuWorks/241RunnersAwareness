/**
 * Google Maps JS map for map.html — public missing cases from GET /api/public/map/missing
 */
(function () {
  'use strict';

  const HOUSTON_LAT = 29.7604;
  const HOUSTON_LNG = -95.3698;

  let map = null;
  let infoWindow = null;
  let markerCluster = null;
  let markers = [];
  let currentData = [];
  let clusteringEnabled = true;
  let heatmap = null;
  let heatmapEnabled = false;

  function getConfig() {
    return window.GOOGLE_MAPS_CONFIG || {};
  }

  function getApiKey() {
    const config = getConfig();
    return config.API_KEY || window.APP_CONFIG?.GOOGLE_MAPS_API_KEY || '';
  }

  function getStatusColor(status) {
    const colors = getConfig().STATUS_COLORS || {};
    return colors[(status || '').toLowerCase()] || '#6b7280';
  }

  function showTokenError() {
    const mapContainer = document.getElementById('map');
    if (!mapContainer) return;
    mapContainer.innerHTML = `
      <div style="display:flex;align-items:center;justify-content:center;height:100%;background:#f3f4f6;border-radius:15px;">
        <div style="text-align:center;padding:40px;max-width:420px;">
          <h3 style="color:#dc2626;margin-bottom:15px;">🔑 Google Maps API Key Required</h3>
          <p style="color:#666;margin-bottom:15px;">Set <code>GOOGLE_MAPS_API_KEY</code> in <code>config.json</code> or <code>google-maps-config.js</code>.</p>
          <p style="color:#888;font-size:0.9rem;">Use the same key as mobile (<code>EXPO_PUBLIC_GOOGLE_MAPS_API_KEY</code>). Enable Maps JavaScript API in Google Cloud Console.</p>
        </div>
      </div>`;
  }

  function createPopupContent(props) {
    const statusColor = getStatusColor(props.currentStatus);
    let lastSeenText = 'Unknown';
    if (props.lastSeenDate) {
      try {
        lastSeenText = new Date(props.lastSeenDate).toLocaleDateString();
      } catch (e) {
        lastSeenText = props.lastSeenDate;
      }
    }
    return `
      <div style="padding:12px;max-width:280px;font-family:Arial,sans-serif;">
        <h3 style="margin:0 0 10px;color:#dc2626;font-size:1.1rem;">${props.displayName}</h3>
        <p style="margin:4px 0;"><strong>Status:</strong>
          <span style="display:inline-block;padding:4px 8px;border-radius:10px;background:${statusColor};color:#fff;font-size:11px;text-transform:uppercase;">${props.currentStatus}</span>
        </p>
        <p style="margin:4px 0;font-size:13px;color:#555;"><strong>Location:</strong> ${props.lastSeenCityState || 'Unknown'}</p>
        <p style="margin:4px 0;font-size:13px;color:#555;"><strong>Last seen:</strong> ${lastSeenText}</p>
        ${props.id ? `<a href="case-detail.html?id=${encodeURIComponent(props.id)}" style="display:inline-block;margin-top:10px;padding:8px 12px;background:#3b82f6;color:#fff;border-radius:8px;font-size:13px;font-weight:600;text-decoration:none;">View case details</a>` : ''}
      </div>`;
  }

  function createMarker(runner) {
    const marker = new google.maps.Marker({
      position: { lat: runner.latitude, lng: runner.longitude },
      map: clusteringEnabled ? null : map,
      title: runner.displayName,
      icon: {
        path: google.maps.SymbolPath.CIRCLE,
        scale: 10,
        fillColor: getStatusColor(runner.currentStatus),
        fillOpacity: 1,
        strokeColor: '#ffffff',
        strokeWeight: 2,
      },
    });

    marker.addListener('click', function () {
      if (!infoWindow) infoWindow = new google.maps.InfoWindow();
      infoWindow.setContent(
        createPopupContent({
          id: runner.id,
          displayName: runner.displayName,
          currentStatus: runner.currentStatus,
          lastSeenCityState: runner.lastSeenCityState,
          lastSeenDate: runner.lastSeenDate,
        })
      );
      infoWindow.open(map, marker);
    });

    return marker;
  }

  function clearMarkers() {
    markers.forEach(function (m) {
      m.setMap(null);
    });
    markers = [];
    if (markerCluster) {
      markerCluster.clearMarkers();
    }
  }

  function displayRunners(runners) {
    if (!map) return;
    clearMarkers();

    markers = (runners || [])
      .filter(function (r) {
        return r.latitude != null && r.longitude != null;
      })
      .map(createMarker);

    if (clusteringEnabled && typeof markerClusterer !== 'undefined') {
      if (!markerCluster) {
        markerCluster = new markerClusterer.MarkerClusterer({
          map: map,
          markers: markers,
        });
      } else {
        markerCluster.addMarkers(markers);
      }
    } else if (!clusteringEnabled) {
      markers.forEach(function (m) {
        m.setMap(map);
      });
    } else {
      markers.forEach(function (m) {
        m.setMap(map);
      });
    }
  }

  function addHoustonOverlay() {
    const radiusMeters = (getConfig().HOUSTON_RADIUS_MILES || 64.4) * 1609.34;
    new google.maps.Circle({
      strokeColor: '#dc2626',
      strokeOpacity: 0.8,
      strokeWeight: 2,
      fillColor: '#dc2626',
      fillOpacity: 0.05,
      map: map,
      center: { lat: HOUSTON_LAT, lng: HOUSTON_LNG },
      radius: radiusMeters,
    });
  }

  function initMap() {
    const apiKey = getApiKey();
    if (!apiKey || apiKey === 'YOUR_GOOGLE_MAPS_API_KEY') {
      showTokenError();
      return;
    }

    if (typeof google === 'undefined' || !google.maps) {
      console.error('Google Maps JS not loaded');
      showTokenError();
      return;
    }

    const config = getConfig();
    const mapContainer = document.getElementById('map');
    if (!mapContainer) return;

    map = new google.maps.Map(mapContainer, {
      center: {
        lat: config.DEFAULT_CENTER?.lat || HOUSTON_LAT,
        lng: config.DEFAULT_CENTER?.lng || HOUSTON_LNG,
      },
      zoom: config.DEFAULT_ZOOM || 10,
      styles: config.MAP_STYLES || [],
      mapTypeId: google.maps.MapTypeId.ROADMAP,
    });

    infoWindow = new google.maps.InfoWindow();
    addHoustonOverlay();
    loadRunnersData();
    bindControls();
  }

  async function loadRunnersData() {
    const loadingEl = document.getElementById('loading');
    const errorEl = document.getElementById('error');
    const noDataEl = document.getElementById('noData');
    const bannerEl = document.getElementById('mapBanner');

    if (loadingEl) loadingEl.style.display = 'block';
    if (errorEl) errorEl.style.display = 'none';
    if (noDataEl) noDataEl.style.display = 'none';
    if (bannerEl) bannerEl.style.display = 'none';

    try {
      const api = window.publicMapApi;
      if (!api) throw new Error('Public map API not loaded.');
      const raw = await api.getMissingMapCases().catch(function () {
        return [];
      });
      const normalized = (raw || []).map(function (item) {
        return api.normalizeMapItem(item);
      });
      const withCoords = normalized.filter(function (r) {
        return r.latitude != null && r.longitude != null;
      });
      currentData = withCoords.map(function (r) {
        return {
          id: r.id,
          fullName: r.displayName,
          displayName: r.displayName,
          currentStatus: (r.status || 'missing').toLowerCase(),
          latitude: r.latitude,
          longitude: r.longitude,
          city: r.lastSeenCity,
          state: r.lastSeenState,
          lastSeenCityState: r.lastSeenCityState,
          lastSeenDate: r.updatedAt,
          dateAdded: r.updatedAt,
        };
      });

      displayRunners(currentData);
      updateStatistics(currentData);
      focusCaseFromQuery();

      if (loadingEl) loadingEl.style.display = 'none';
      if (currentData.length === 0 && noDataEl) noDataEl.style.display = 'block';
      if (currentData.length > 0 && bannerEl) {
        bannerEl.textContent =
          'Map locations are approximate. Cases without a reported location are shown in the general Houston area.';
        bannerEl.style.display = 'block';
      }
    } catch (error) {
      console.error('Error loading map data:', error);
      if (loadingEl) loadingEl.style.display = 'none';
      if (errorEl) {
        errorEl.style.display = 'block';
        errorEl.textContent = 'Error loading map data. Please try again later.';
      }
    }
  }

  function updateStatistics(data) {
    data = data || currentData;
    const stats = {
      total: data.length,
      missing: data.filter(function (r) {
        return r.currentStatus === 'missing';
      }).length,
      found: data.filter(function (r) {
        return r.currentStatus === 'found';
      }).length,
      safe: data.filter(function (r) {
        return r.currentStatus === 'safe';
      }).length,
      urgent: data.filter(function (r) {
        return r.currentStatus === 'urgent';
      }).length,
      recent: data.filter(function (r) {
        const daysSince = Math.floor((Date.now() - new Date(r.dateAdded)) / 86400000);
        return daysSince <= 30;
      }).length,
    };

    const ids = ['totalCases', 'missingCases', 'foundCases', 'safeCases', 'recentCases', 'urgentCases'];
    const vals = [stats.total, stats.missing, stats.found, stats.safe, stats.recent, stats.urgent];
    ids.forEach(function (id, i) {
      const el = document.getElementById(id);
      if (el) el.textContent = String(vals[i]);
    });
  }

  function centerMap() {
    if (!map || !currentData.length) return;
    const bounds = new google.maps.LatLngBounds();
    currentData.forEach(function (r) {
      bounds.extend({ lat: r.latitude, lng: r.longitude });
    });
    map.fitBounds(bounds, 48);
  }

  function focusHouston() {
    if (!map) return;
    map.panTo({ lat: HOUSTON_LAT, lng: HOUSTON_LNG });
    map.setZoom(10);
  }

  function toggleClusters() {
    if (!map) return;
    clusteringEnabled = !clusteringEnabled;
    const btn = document.getElementById('toggleClustersBtn');
    if (heatmapEnabled) toggleHeatmap();
    displayRunners(currentData);
    if (btn) btn.textContent = clusteringEnabled ? '📊 Disable Clusters' : '📊 Enable Clusters';
  }

  function toggleHeatmap() {
    if (!map) return;
    const btn = document.getElementById('heatmapBtn');
    if (heatmapEnabled) {
      if (heatmap) {
        heatmap.setMap(null);
        heatmap = null;
      }
      displayRunners(currentData);
      heatmapEnabled = false;
      if (btn) btn.textContent = '🔥 Heat Map';
    } else {
      clearMarkers();
      const heatmapData = currentData.map(function (r) {
        return new google.maps.LatLng(r.latitude, r.longitude);
      });
      const config = getConfig();
      heatmap = new google.maps.visualization.HeatmapLayer({
        data: heatmapData,
        map: map,
        radius: config.HEATMAP_CONFIG?.radius || 20,
        opacity: config.HEATMAP_CONFIG?.opacity || 0.6,
      });
      heatmapEnabled = true;
      if (btn) btn.textContent = '🗺️ Normal Map';
    }
  }

  function filterRunners() {
    const statusFilter = document.getElementById('statusFilter')?.value || '';
    const timeFilter = document.getElementById('timeFilter')?.value || '';
    const searchTerm = (document.getElementById('searchBox')?.value || '').toLowerCase();

    let filtered = currentData.slice();

    if (statusFilter) {
      filtered = filtered.filter(function (r) {
        return r.currentStatus === statusFilter;
      });
    }

    if (timeFilter) {
      const days = { '24h': 1, '7d': 7, '30d': 30, '90d': 90 }[timeFilter];
      if (days) {
        const cutoff = Date.now() - days * 86400000;
        filtered = filtered.filter(function (r) {
          return new Date(r.dateAdded).getTime() >= cutoff;
        });
      }
    }

    if (searchTerm) {
      filtered = filtered.filter(function (r) {
        return (
          (r.fullName && r.fullName.toLowerCase().includes(searchTerm)) ||
          (r.city && r.city.toLowerCase().includes(searchTerm)) ||
          String(r.id).includes(searchTerm)
        );
      });
    }

    displayRunners(filtered);
    updateStatistics(filtered);
  }

  function focusCaseFromQuery() {
    const params = new URLSearchParams(window.location.search);
    const caseId = params.get('case');
    if (!caseId || !map) return;
    const match = currentData.find(function (r) {
      return String(r.id) === caseId;
    });
    if (match) {
      map.panTo({ lat: match.latitude, lng: match.longitude });
      map.setZoom(14);
    }
  }

  function bindControls() {
    const refreshBtn = document.getElementById('refreshBtn');
    const houstonBtn = document.getElementById('houstonBtn');
    const statusFilter = document.getElementById('statusFilter');
    const timeFilter = document.getElementById('timeFilter');
    const searchBox = document.getElementById('searchBox');
    const centerMapBtn = document.getElementById('centerMapBtn');
    const toggleClustersBtn = document.getElementById('toggleClustersBtn');
    const heatmapBtn = document.getElementById('heatmapBtn');
    const reportRunnerBtn = document.getElementById('reportRunnerBtn');

    if (refreshBtn) refreshBtn.addEventListener('click', loadRunnersData);
    if (houstonBtn) houstonBtn.addEventListener('click', focusHouston);
    if (statusFilter) statusFilter.addEventListener('change', filterRunners);
    if (timeFilter) timeFilter.addEventListener('change', filterRunners);
    if (searchBox) searchBox.addEventListener('input', filterRunners);
    if (centerMapBtn) centerMapBtn.addEventListener('click', centerMap);
    if (toggleClustersBtn) toggleClustersBtn.addEventListener('click', toggleClusters);
    if (heatmapBtn) heatmapBtn.addEventListener('click', toggleHeatmap);
    if (reportRunnerBtn) {
      reportRunnerBtn.addEventListener('click', function () {
        window.location.href = 'https://241runnersawareness.org/report-case.html';
      });
    }

    window.addEventListener('scroll', function () {
      const header = document.querySelector('header');
      if (header) header.classList.toggle('scrolled', window.scrollY > 50);
    });

    const hamburger = document.querySelector('.hamburger');
    const nav = document.querySelector('nav');
    if (hamburger && nav) {
      hamburger.addEventListener('click', function () {
        const isExpanded = hamburger.getAttribute('aria-expanded') === 'true';
        hamburger.setAttribute('aria-expanded', String(!isExpanded));
        hamburger.classList.toggle('active');
        nav.classList.toggle('active');
      });
      document.querySelectorAll('nav a').forEach(function (link) {
        link.addEventListener('click', function () {
          hamburger.classList.remove('active');
          nav.classList.remove('active');
        });
      });
    }
  }

  function loadGoogleMapsApi() {
    const apiKey = getApiKey();
    if (!apiKey || apiKey === 'YOUR_GOOGLE_MAPS_API_KEY') {
      showTokenError();
      return;
    }

    if (typeof google !== 'undefined' && google.maps) {
      initMap();
      return;
    }

    window.initGoogleMap = initMap;
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&libraries=geometry,visualization&callback=initGoogleMap&loading=async`;
    script.async = true;
    script.defer = true;
    script.onerror = function () {
      console.error('Failed to load Google Maps API');
      showTokenError();
    };
    document.head.appendChild(script);
  }

  document.addEventListener('DOMContentLoaded', function () {
    setTimeout(loadGoogleMapsApi, 300);
  });
})();
