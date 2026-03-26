document.addEventListener('DOMContentLoaded', () => {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach((alert) => {
        if (alert.classList.contains('alert-info')) return;
        setTimeout(() => {
            alert.style.transition = 'opacity .35s ease, transform .35s ease';
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-6px)';
        }, 4500);
    });
});
