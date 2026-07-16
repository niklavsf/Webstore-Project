document.addEventListener("DOMContentLoaded", function () {

    /* =========================
       DESKTOP PANEL TOGGLE
       ========================= */

    const toggleBtn = document.getElementById("catToggleBtn");
    const panel = document.getElementById("catDesktopPanel");

    if (toggleBtn && panel) {

        toggleBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            panel.classList.toggle("d-none");
        });

        // Close when clicking outside
        document.addEventListener("click", function (e) {
            if (!panel.contains(e.target) && e.target !== toggleBtn) {
                panel.classList.add("d-none");
            }
        });

        // Prevent inside clicks from closing
        panel.addEventListener("click", function (e) {
            e.stopPropagation();
        });
    }

    /* =========================
       MEGA MENU LOGIC (DESKTOP)
       ========================= */

    if (window.innerWidth < 992) return;

    if (!window.CATMENU_DATA) return;

    const root = document.getElementById("catMenuRoot");
    const col2 = document.getElementById("catMenuCol2");
    const col3 = document.getElementById("catMenuCol3");
    const subList = document.getElementById("catMenuSubList");
    const leafList = document.getElementById("catMenuLeafList");

    if (!root || !col2 || !col3) return;

    function clearActive(sel) {
        root.querySelectorAll(sel).forEach(x => x.classList.remove("active"));
    }

    function resetCols() {
        col2.classList.add("d-none");
        col3.classList.add("d-none");
        subList.innerHTML = "";
        leafList.innerHTML = "";
    }

    function renderSub(topId) {
        resetCols();
        const top = window.CATMENU_DATA.find(x => x.id === topId);
        if (!top || !top.children.length) return;

        subList.innerHTML =
            `<a class="list-group-item list-group-item-action"
                href="/Products?categoryId=${topId}">
                Visi kategorijas produkti
            </a>`;

        top.children.forEach(mid => {
            subList.insertAdjacentHTML("beforeend",
                `<button type="button"
                         class="list-group-item list-group-item-action catmenu-mid"
                         data-top-id="${topId}"
                         data-mid-id="${mid.id}">
                    <span>${mid.name}</span><span class="catmenu-arrow"></span>
                 </button>`);
        });

        col2.classList.remove("d-none");
    }

    function renderLeaf(topId, midId) {
        leafList.innerHTML = "";
        const top = window.CATMENU_DATA.find(x => x.id === topId);
        const mid = top?.children.find(x => x.id === midId);
        if (!mid || !mid.children.length) return;

        mid.children.forEach(leaf => {
            leafList.insertAdjacentHTML("beforeend",
                `<a class="list-group-item list-group-item-action"
                    href="/Products?categoryId=${leaf.id}">
                    ${leaf.name}
                 </a>`);
        });

        col3.classList.remove("d-none");
    }

    root.querySelectorAll(".catmenu-top").forEach(btn => {
        btn.addEventListener("click", function () {
            clearActive(".catmenu-top");
            clearActive(".catmenu-mid");
            this.classList.add("active");
            renderSub(parseInt(this.dataset.topId));
        });
    });

    root.addEventListener("click", function (e) {
        const mid = e.target.closest(".catmenu-mid");
        if (!mid) return;

        clearActive(".catmenu-mid");
        mid.classList.add("active");

        renderLeaf(
            parseInt(mid.dataset.topId),
            parseInt(mid.dataset.midId)
        );
    });
});
