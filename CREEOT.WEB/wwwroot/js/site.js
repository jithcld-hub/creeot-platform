document.addEventListener('DOMContentLoaded', () => {
    // Dropdown toggles
    const profileBtn = document.getElementById('profileDropdownBtn');
    const profileContent = document.getElementById('profileDropdownContent');

    if (profileBtn && profileContent) {
        profileBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            const isOpen = profileContent.classList.toggle('show');
            profileBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        });
    }

    // Dismiss on click-outside
    document.addEventListener('click', (e) => {
        if (profileContent && profileContent.classList.contains('show')) {
            if (!profileContent.contains(e.target) && !profileBtn.contains(e.target)) {
                profileContent.classList.remove('show');
                profileBtn?.setAttribute('aria-expanded', 'false');
            }
        }
    });

    // Toast auto-dismissal
    const toast = document.getElementById('statusToast');
    if (toast) {
        setTimeout(() => {
            toast.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(-10px)';
            setTimeout(() => toast.remove(), 400);
        }, 5000);
    }
});