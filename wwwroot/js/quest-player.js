document.addEventListener('DOMContentLoaded', () => {
    const player = document.querySelector('.quest-player');
    if (!player) return;

    const requestButton = document.getElementById('requestLocationBtn');
    const status = document.getElementById('gpsStatus');
    const answerForm = document.getElementById('answerForm');

    const questId = Number(player.dataset.questId);
    const pointId = Number(player.dataset.pointId);

    const setFormEnabled = (enabled) => {
        answerForm.classList.toggle('is-disabled', !enabled);
        answerForm.querySelectorAll('input, button').forEach((element) => {
            if (element.type === 'hidden') return;
            element.disabled = !enabled;
        });
        player.dataset.verified = enabled ? 'true' : 'false';
    };

    const setStatus = (message, type) => {
        status.textContent = message;
        status.className = 'gps-status';
        status.classList.add(type);
    };

    if (player.dataset.verified === 'true') {
        setFormEnabled(true);
        setStatus('Локация уже подтверждена. Можно отвечать на задание.', 'gps-success');
    } else {
        setFormEnabled(false);
    }

    requestButton?.addEventListener('click', () => {
        if (!navigator.geolocation) {
            setStatus('Твой браузер не поддерживает геолокацию.', 'gps-error');
            return;
        }

        requestButton.disabled = true;
        requestButton.textContent = 'Определяем местоположение…';
        setStatus('Запрашиваем доступ к геолокации…', 'gps-neutral');

        navigator.geolocation.getCurrentPosition(async (position) => {
            try {
                const response = await fetch('/Quest/VerifyLocation', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        id: questId,
                        pointId: pointId,
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude
                    })
                });

                const data = await response.json();
                if (!data.success) {
                    setFormEnabled(false);
                    setStatus(data.message || 'Не удалось проверить локацию.', 'gps-error');
                    return;
                }

                const distanceText = typeof data.distanceMeters === 'number'
                    ? ` Расстояние до точки: ${data.distanceMeters} м.`
                    : '';

                if (data.withinRadius) {
                    setFormEnabled(true);
                    setStatus(`${data.message}${distanceText}`, 'gps-success');
                } else {
                    setFormEnabled(false);
                    setStatus(`${data.message}${distanceText}`, 'gps-error');
                }
            } catch {
                setFormEnabled(false);
                setStatus('Ошибка при проверке координат на сервере.', 'gps-error');
            } finally {
                requestButton.disabled = false;
                requestButton.textContent = 'Проверить мою геолокацию';
            }
        }, (error) => {
            requestButton.disabled = false;
            requestButton.textContent = 'Проверить мою геолокацию';

            switch (error.code) {
                case error.PERMISSION_DENIED:
                    setStatus('Доступ к геолокации отклонён. Разреши его в браузере.', 'gps-error');
                    break;
                case error.POSITION_UNAVAILABLE:
                    setStatus('Не удалось определить координаты устройства.', 'gps-error');
                    break;
                case error.TIMEOUT:
                    setStatus('Определение геолокации заняло слишком много времени.', 'gps-error');
                    break;
                default:
                    setStatus('Произошла неизвестная ошибка геолокации.', 'gps-error');
                    break;
            }
        }, {
            enableHighAccuracy: true,
            timeout: 12000,
            maximumAge: 0
        });
    });
});
