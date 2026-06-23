/**
 * Mapbox GL JS map for map.html — public missing cases from GET /api/public/map/missing
 */
(function () {
  'use strict';

  const HOUSTON_LAT = 29.7604;
  const HOUSTON_LNG = -95.3698;

  let map = null;
  let currentData = [];
  let clusteringEnabled = true;
  let heatmapEnabled = false;
  let popup = null;
  let layersInitialized = false;

  function getConfig() {
    return window.MAPBOX_CONFIG || {};
  }

  function getAccessToken() {
    const config = getConfig();
    return config.ACCESS_TOKEN || window.APP_CONFIG?.MAPBOX_ACCESS_TOKEN || '';
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
          <h3 style="color:#dc2626;margin-bottom:15px;">🔑 Mapbox Access Token Required</h3>
          <p style="color:#666;margin-bottom:15px;">Set <code>MAPBOX_ACCESS_TOKEN</code> in <code>config.json</code> or <code>mapbox-config.js</code>.</p>
          <p style="color:#888;font-size:0.9rem;">Use the same public token as mobile (<code>EXPO_PUBLIC_MAPBOX_ACCESS_TOKEN</code>).</p>
        </div>
      </div>`;
  }

  function runnersToGeoJSON(runners) {
    return {
      type: 'FeatureCollection',
      features: (runners || [])
        .filter(function (r) {
          return r.latitude != null && r.longitude != null;
        })
        .map(function (r) {
          return {
            type: 'Feature',
            geometry: {
              type: 'Point',
              coordinates: [r.longitude, r.latitude],
            },
            properties: {
              id: r.id,
              displayName: r.displayName || r.fullName || 'Unknown',
              currentStatus: (r.currentStatus || 'missing').toLowerCase(),
              lastSeenCityState: r.lastSeenCityState || '',
              lastSeenDate: r.lastSeenDate || '',
              color: getStatusColor(r.currentStatus),
            },
          };
        }),
    };
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

  function rebuildCasesSource(runners, withCluster) {
    if (!map) return;
    heatmapEnabled = false;
    if (map.getLayer('cases-heat')) map.removeLayer('cases-heat');
    ['cases-unclustered', 'cases-cluster-count', 'cases-clusters'].forEach(function (id) {
      if (map.getLayer(id)) map.removeLayer(id);
    });
    if (map.getSource('cases')) map.removeSource('cases');

    map.addSource('cases', {
      type: 'geojson',
      data: runnersToGeoJSON(runners),
      cluster: withCluster,
      clusterMaxZoom: getConfig().CLUSTER_MAX_ZOOM || 14,
      clusterRadius: getConfig().CLUSTER_RADIUS || 50,
    });

    if (withCluster) {
      map.addLayer({
        id: 'cases-clusters',
        type: 'circle',
        source: 'cases',
        filter: ['has', 'point_count'],
        paint: {
          'circle-color': ['step', ['get', 'point_count'], '#fbbf24', 10, '#f59e0b', 25, '#dc2626'],
          'circle-radius': ['step', ['get', 'point_count'], 18, 10, 22, 25, 28],
          'circle-stroke-width': 2,
          'circle-stroke-color': '#fff',
        },
      });
      map.addLayer({
        id: 'cases-cluster-count',
        type: 'symbol',
        source: 'cases',
        filter: ['has', 'point_count'],
        layout: {
          'text-field': ['get', 'point_count_abbreviated'],
          'text-size': 12,
        },
        paint: { 'text-color': '#ffffff' },
      });
    }

    map.addLayer({
      id: 'cases-unclustered',
      type: 'circle',
      source: 'cases',
      filter: withCluster ? ['!', ['has', 'point_count']] : undefined,
      paint: {
        'circle-color': ['get', 'color'],
        'circle-radius': 10,
        'circle-stroke-width': 2,
        'circle-stroke-color': '#ffffff',
      },
    });
  }

  function bindCaseLayerEvents() {
    map.on('click', 'cases-clusters', function (e) {
      const features = map.queryRenderedFeatures(e.point, { layers: ['cases-clusters'] });
      if (!features.length) return;
      const clusterId = features[0].properties.cluster_id;
      map.getSource('cases').getClusterExpansionZoom(clusterId, function (err, zoom) {
        if (err) return;
        map.easeTo({ center: features[0].geometry.coordinates, zoom: zoom });
      });
    });

    map.on('click', 'cases-unclustered', function (e) {
      if (!e.features || !e.features.length) return;
      const props = e.features[0].properties;
      const coordinates = e.features[0].geometry.coordinates.slice();
      if (popup) popup.remove();
      popup = new mapboxgl.Popup({ offset: 12 })
        .setLngLat(coordinates)
        .setHTML(createPopupContent(props))
        .addTo(map);
    });

    ['cases-clusters', 'cases-unclustered'].forEach(function (layer) {
      map.on('mouseenter', layer, function () {
        map.getCanvas().style.cursor = 'pointer';
      });
      map.on('mouseleave', layer, function () {
        map.getCanvas().style.cursor = '';
      });
    });
  }

  function addMapLayers() {
    if (!map || layersInitialized) return;
    rebuildCasesSource([], clusteringEnabled);
    bindCaseLayerEvents();
    layersInitialized = true;
  }

  function displayRunners(runners) {
    if (!map) return;
    if (!map.getSource('cases')) {
      rebuildCasesSource(runners, clusteringEnabled);
      return;
    }
    map.getSource('cases').setData(runnersToGeoJSON(runners));
  }

  function initMap() {
    const token = getAccessToken();
    if (!token || token === 'YOUR_MAPBOX_ACCESS_TOKEN') {
      showTokenError();
      return;
    }

    if (typeof mapboxgl === 'undefined') {
      console.error('Mapbox GL JS not loaded');
      showTokenError();
      return;
    }

    const config = getConfig();
    mapboxgl.accessToken = token;

    map = new mapboxgl.Map({
      container: 'map',
      style: config.STYLE_URL || 'mapbox://styles/mapbox/streets-v12',
      center: [config.DEFAULT_CENTER.lng, config.DEFAULT_CENTER.lat],
      zoom: config.DEFAULT_ZOOM || 10,
    });

    map.addControl(new mapboxgl.NavigationControl(), 'top-right');
    map.addControl(
      new mapboxgl.GeolocateControl({
        positionOptions: { enableHighAccuracy: true },
        trackUserLocation: true,
        showUserHeading: true,
      }),
      'top-right'
    );

    map.on('load', function () {
      addMapLayers();
      addHoustonOverlay();
      loadRunnersData();
      bindControls();
    });
  }

  function addHoustonOverlay() {
    const radiusMeters = (getConfig().HOUSTON_RADIUS_MILES || 64.4) * 1609.34;
    const points = 64;
    const coords = [];
    for (let i = 0; i < points; i++) {
      const angle = (i / points) * 2 * Math.PI;
      const dx = (radiusMeters / 111320) * Math.cos(angle);
      const dy = (radiusMeters / 110540) * Math.sin(angle);
      coords.push([HOUSTON_LNG + dy, HOUSTON_LAT + dx]);
    }
    coords.push(coords[0]);

    if (map.getSource('houston-area')) return;
    map.addSource('houston-area', {
      type: 'geojson',
      data: {
        type: 'Feature',
        geometry: { type: 'Polygon', coordinates: [coords] },
      },
    });
    map.addLayer({
      id: 'houston-fill',
      type: 'fill',
      source: 'houston-area',
      paint: { 'fill-color': '#dc2626', 'fill-opacity': 0.05 },
    });
    map.addLayer({
      id: 'houston-outline',
      type: 'line',
      source: 'houston-area',
      paint: { 'line-color': '#dc2626', 'line-width': 2, 'line-opacity': 0.8 },
    });
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
    const bounds = new mapboxgl.LngLatBounds();
    currentData.forEach(function (r) {
      bounds.extend([r.longitude, r.latitude]);
    });
    map.fitBounds(bounds, { padding: 48, maxZoom: 14 });
  }

  function focusHouston() {
    if (!map) return;
    map.flyTo({ center: [HOUSTON_LNG, HOUSTON_LAT], zoom: 10 });
  }

  function toggleClusters() {
    if (!map) return;
    clusteringEnabled = !clusteringEnabled;
    const btn = document.getElementById('toggleClustersBtn');
    rebuildCasesSource(currentData, clusteringEnabled);
    if (btn) btn.textContent = clusteringEnabled ? '📊 Disable Clusters' : '📊 Enable Clusters';
  }

  function toggleHeatmap() {
    if (!map) return;
    const btn = document.getElementById('heatmapBtn');
    if (heatmapEnabled) {
      if (map.getLayer('cases-heat')) map.removeLayer('cases-heat');
      if (map.getLayer('cases-unclustered')) map.setLayoutProperty('cases-unclustered', 'visibility', 'visible');
      if (map.getLayer('cases-clusters')) map.setLayoutProperty('cases-clusters', 'visibility', 'visible');
      if (map.getLayer('cases-cluster-count')) map.setLayoutProperty('cases-cluster-count', 'visibility', 'visible');
      heatmapEnabled = false;
      if (btn) btn.textContent = '🔥 Heat Map';
    } else {
      if (map.getLayer('cases-unclustered')) map.setLayoutProperty('cases-unclustered', 'visibility', 'none');
      if (map.getLayer('cases-clusters')) map.setLayoutProperty('cases-clusters', 'visibility', 'none');
      if (map.getLayer('cases-cluster-count')) map.setLayoutProperty('cases-cluster-count', 'visibility', 'none');
      if (!map.getLayer('cases-heat')) {
        map.addLayer({
          id: 'cases-heat',
          type: 'heatmap',
          source: 'cases',
          maxzoom: 15,
          paint: {
            'heatmap-weight': 1,
            'heatmap-intensity': 1,
            'heatmap-radius': getConfig().HEATMAP_CONFIG?.radius || 20,
            'heatmap-opacity': getConfig().HEATMAP_CONFIG?.opacity || 0.6,
          },
        });
      }
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
      map.flyTo({ center: [match.longitude, match.latitude], zoom: 14 });
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

  document.addEventListener('DOMContentLoaded', function () {
    setTimeout(initMap, 300);
  });
})();
