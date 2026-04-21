(function () {
    function parseJsonScript(id) {
        const el = document.getElementById(id);
        if (!el) return [];
        try {
            return JSON.parse(el.textContent || '[]');
        } catch {
            return [];
        }
    }

    function createPopup(point, isActive) {
        const badge = isActive ? '<div style="margin-top:8px;font-weight:700;color:#d97706;">Текущая точка</div>' : '';
        const meta = point.address ? `<div style="margin-top:6px;color:#607089;">${point.address}</div>` : '';
        return `<strong>${point.name || 'Точка маршрута'}</strong>${meta}${badge}`;
    }

    function buildMap(root) {
        if (!window.L) return;

        const jsonId = root.dataset.jsonId;
        const activePointId = Number(root.dataset.activePointId || 0);
        const points = parseJsonScript(jsonId)
            .filter(p => Number.isFinite(Number(p.latitude)) && Number.isFinite(Number(p.longitude)))
            .map(p => ({ ...p, latitude: Number(p.latitude), longitude: Number(p.longitude), order: Number(p.order || 0) }));

        if (!points.length) return;

        const map = L.map(root, {
            scrollWheelZoom: false,
            zoomControl: true
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        const bounds = [];
        const polylinePoints = [];

        points.sort((a, b) => a.order - b.order).forEach((point, index) => {
            const coords = [point.latitude, point.longitude];
            bounds.push(coords);
            polylinePoints.push(coords);

            const isActive = activePointId > 0 && point.id === activePointId;
            const iconHtml = isActive
                ? `<div class="map-marker map-marker-active"><span>${point.order || index + 1}</span></div>`
                : `<div class="map-marker"><span>${point.order || index + 1}</span></div>`;

            const marker = L.marker(coords, {
                icon: L.divIcon({
                    className: 'qq-map-marker-wrap',
                    html: iconHtml,
                    iconSize: [34, 34],
                    iconAnchor: [17, 17]
                })
            }).addTo(map);

            marker.bindPopup(createPopup(point, isActive));

            if (isActive) {
                L.circle(coords, {
                    radius: Number(point.radiusMeters || 150),
                    weight: 2,
                    fillOpacity: 0.12
                }).addTo(map);
            }
        });

        if (polylinePoints.length > 1) {
            L.polyline(polylinePoints, {
                weight: 4,
                opacity: 0.75
            }).addTo(map);
        }

        if (bounds.length === 1) {
            map.setView(bounds[0], 15);
        } else {
            map.fitBounds(bounds, { padding: [30, 30] });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-quest-map]').forEach(buildMap);
    });
})();
