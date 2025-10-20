<script>
        // Toggle sidebar on mobile
    document.getElementById('sidebarToggle')?.addEventListener('click', function() {
            const sidebar = document.getElementById('leftSidebar');
    sidebar.classList.toggle('show');
        });

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function(event) {
            const sidebar = document.getElementById('leftSidebar');
    const toggle = document.getElementById('sidebarToggle');

    if (window.innerWidth < 992) {
                if (!sidebar.contains(event.target) && !toggle?.contains(event.target)) {
        sidebar.classList.remove('show');
                }
            }
        });

    // Highlight active menu
    document.addEventListener('DOMContentLoaded', function() {
            const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');

            navLinks.forEach(link => {
                if (link.getAttribute('href') === currentPath) {
        link.classList.add('active');
                }
            });
        });

        // Close sidebar when nav link clicked on mobile
        document.querySelectorAll('.sidebar-nav .nav-link').forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                document.getElementById('leftSidebar').classList.remove('show');
            }
        });
        });
</script>