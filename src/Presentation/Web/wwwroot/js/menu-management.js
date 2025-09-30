(function () {
    "use strict";

    const selectors = {
        root: "[data-menu-details]",
        alerts: "#menu-alerts",
        categoryCreatePanel: "[data-menu-panel='category-create']",
        categoryCreateForm: "[data-menu-form='category-create']",
        categoryEditForm: "[data-menu-form='category-edit']",
        itemCreateForm: "[data-menu-form='item-create']",
        itemEditForm: "[data-menu-form='item-edit']",
        publishButton: "[data-menu-action='publish-menu']",
        showCreateCategoryButton: "[data-menu-action='show-create-category']",
        hideCreateCategoryButton: "[data-menu-action='hide-create-category']",
        archiveCategoryButton: "[data-menu-action='archive-category']",
        restoreCategoryButton: "[data-menu-action='restore-category']",
        reorderCategoriesContainer: "[data-sortable='categories']",
        reorderItemsContainer: "[data-sortable='items']",
        toggleItemButton: "[data-menu-action='toggle-item-availability']",
        removeItemButton: "[data-menu-action='remove-item']"
    };

    const antiForgerySelector = "#menu-anti-forgery input[name='__RequestVerificationToken']";

    const state = {
        root: null,
        menuId: null,
        tenantId: null,
        sortableInstances: []
    };

    document.addEventListener("DOMContentLoaded", initialize);

    function initialize() {
        state.root = document.querySelector(selectors.root);
        if (!state.root) {
            return;
        }

        state.menuId = state.root.getAttribute("data-menu-id");
        if (!state.menuId) {
            console.warn("Menu management script: missing menu id");
            return;
        }

        state.tenantId = state.root.getAttribute("data-tenant-id") || null;
        wireCategoryPanelToggles();
        wireForms();
        wireCategoryActions();
        wireItemActions();
        wirePublishButton();
        initializeSortables();
    }

    function wireCategoryPanelToggles() {
        const showButton = state.root.querySelector(selectors.showCreateCategoryButton);
        const panel = state.root.querySelector(selectors.categoryCreatePanel);
        const hideButton = panel ? panel.querySelector(selectors.hideCreateCategoryButton) : null;

        if (showButton && panel) {
            showButton.addEventListener("click", () => toggleCollapse(panel, true));
        }

        if (hideButton && panel) {
            hideButton.addEventListener("click", () => toggleCollapse(panel, false));
        }
    }

    function toggleCollapse(element, show) {
        const collapse = bootstrap.Collapse.getOrCreateInstance(element);
        if (show) {
            collapse.show();
        } else {
            collapse.hide();
        }
    }

    function wireForms() {
        const root = state.root;
        root.querySelectorAll(selectors.categoryCreateForm).forEach(form => attachFormHandler(form, onCategoryFormSuccess));
        root.querySelectorAll(selectors.categoryEditForm).forEach(form => attachFormHandler(form, onCategoryFormSuccess));
        root.querySelectorAll(selectors.itemCreateForm).forEach(form => attachFormHandler(form, onItemFormSuccess));
        root.querySelectorAll(selectors.itemEditForm).forEach(form => attachFormHandler(form, onItemFormSuccess));
    }

    function attachFormHandler(form, onSuccess) {
        form.addEventListener("submit", event => {
            event.preventDefault();
            const submitButton = form.querySelector("button[type='submit']");
            setLoading(submitButton, true);

            const formData = new FormData(form);
            const url = form.getAttribute("action");
            const method = form.getAttribute("method") || "post";

            fetch(url, {
                method: method.toUpperCase(),
                body: formData,
                headers: buildHeaders()
            })
                .then(response => handlePartialResponse(response))
                .then(html => {
                    if (html) {
                        onSuccess(form, html);
                        showAlert("success", "تغییرات با موفقیت ثبت شد.");
                    }
                })
                .catch(error => {
                    console.error("Menu management form error", error);
                    showAlert("danger", "در ثبت تغییرات خطایی رخ داد.");
                })
                .finally(() => setLoading(submitButton, false));
        });
    }

    function onCategoryFormSuccess(form, html) {
        replaceCategoryList(html);
        collapseParent(form);
        initializeSortables();
    }

    function onItemFormSuccess(form, html) {
        replaceCategoryList(html);
        collapseParent(form);
        initializeSortables();
    }

    function collapseParent(form) {
        const collapseContainer = form.closest(".collapse");
        if (collapseContainer) {
            toggleCollapse(collapseContainer, false);
        }
    }

    function wireCategoryActions() {
        state.root.addEventListener("click", event => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (target.matches(selectors.archiveCategoryButton)) {
                handleCategoryCommand(target, "Archive");
            } else if (target.matches(selectors.restoreCategoryButton)) {
                handleCategoryCommand(target, "Restore");
            }
        });
    }

    function handleCategoryCommand(button, action) {
        const menuId = button.getAttribute("data-menu-id");
        const categoryId = button.getAttribute("data-category-id");
        if (!menuId || !categoryId) {
            return;
        }

        setLoading(button, true);

        const url = buildUrl(`/MenuCategories/${action}?menuId=${menuId}&categoryId=${categoryId}`);

        fetch(url, {
            method: "POST",
            headers: buildHeaders()
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    replaceCategoryList(html);
                    initializeSortables();
                    showAlert("success", "تغییرات دسته ثبت شد.");
                }
            })
            .catch(error => {
                console.error("Menu management category command error", error);
                showAlert("danger", "در پردازش درخواست دسته خطایی رخ داد.");
            })
            .finally(() => setLoading(button, false));
    }

    function wireItemActions() {
        state.root.addEventListener("click", event => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (target.matches(selectors.toggleItemButton)) {
                handleToggleItem(target);
            } else if (target.matches(selectors.removeItemButton)) {
                handleRemoveItem(target);
            }
        });
    }

    function handleToggleItem(button) {
        const menuId = button.getAttribute("data-menu-id");
        const categoryId = button.getAttribute("data-category-id");
        const itemId = button.getAttribute("data-item-id");
        const current = button.getAttribute("data-current") === "true";
        if (!menuId || !categoryId || !itemId) {
            return;
        }

        setLoading(button, true);

        const url = buildUrl(`/MenuItems/SetAvailability?menuId=${menuId}&categoryId=${categoryId}&itemId=${itemId}`);

        fetch(url, {
            method: "POST",
            headers: buildJsonHeaders(),
            body: JSON.stringify({ isAvailable: !current })
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    replaceCategoryList(html);
                    initializeSortables();
                    showAlert("success", "وضعیت آیتم به‌روزرسانی شد.");
                }
            })
            .catch(error => {
                console.error("Menu management toggle item error", error);
                showAlert("danger", "در تغییر وضعیت آیتم خطایی رخ داد.");
            })
            .finally(() => setLoading(button, false));
    }

    function handleRemoveItem(button) {
        if (!confirm("آیا از حذف این آیتم مطمئن هستید؟")) {
            return;
        }

        const menuId = button.getAttribute("data-menu-id");
        const categoryId = button.getAttribute("data-category-id");
        const itemId = button.getAttribute("data-item-id");
        if (!menuId || !categoryId || !itemId) {
            return;
        }

        setLoading(button, true);

        const url = buildUrl(`/MenuItems/Remove?menuId=${menuId}&categoryId=${categoryId}&itemId=${itemId}`);

        fetch(url, {
            method: "POST",
            headers: buildHeaders()
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    replaceCategoryList(html);
                    initializeSortables();
                    showAlert("success", "آیتم با موفقیت حذف شد.");
                }
            })
            .catch(error => {
                console.error("Menu management remove item error", error);
                showAlert("danger", "در حذف آیتم خطایی رخ داد.");
            })
            .finally(() => setLoading(button, false));
    }

    function wirePublishButton() {
        const button = state.root.querySelector(selectors.publishButton);
        if (!button) {
            return;
        }

        button.addEventListener("click", () => {
            setLoading(button, true);
            const url = buildUrl(`/Menus/Publish?menuId=${state.menuId}`);

            fetch(url, {
                method: "POST",
                headers: buildHeaders()
            })
                .then(response => {
                    if (response.ok) {
                        return response.json();
                    }

                    return response.json().then(payload => {
                        throw new Error(payload.message || "publish failed");
                    });
                })
                .then(data => {
                    const versionElement = state.root.querySelector("[data-published-version]");
                    if (versionElement && data && typeof data.version === "number") {
                        versionElement.textContent = `v${data.version}`;
                    }
                    showAlert("success", "منو با موفقیت منتشر شد.");
                })
                .catch(error => {
                    console.error("Menu publish error", error);
                    showAlert("danger", "در انتشار منو خطایی رخ داد.");
                })
                .finally(() => setLoading(button, false));
        });
    }

    function buildUrl(path) {
        if (path.startsWith("http")) {
            return path;
        }

        const base = state.root.getAttribute("data-base-url") || "";
        return `${base}${path}`;
    }

    function handlePartialResponse(response) {
        if (response.ok) {
            return response.text();
        }

        return response.json()
            .then(payload => {
                const message = payload?.message || "خطای نامشخص";
                showAlert("danger", message);
                throw new Error(message);
            })
            .catch(error => {
                showAlert("danger", "خطای نامشخصی رخ داد.");
                throw error;
            });
    }

    function replaceCategoryList(html) {
        if (!html) {
            return;
        }

        const wrapper = document.querySelector("#menu-categories-wrapper");
        if (!wrapper) {
            return;
        }

        wrapper.innerHTML = html;
        wireForms();
    }

    function initializeSortables() {
        state.sortableInstances.forEach(instance => instance.destroy());
        state.sortableInstances = [];

        const categoryContainer = state.root.querySelector(selectors.reorderCategoriesContainer);
        if (categoryContainer) {
            const sortable = new Sortable(categoryContainer, {
                animation: 150,
                handle: ".card-header",
                direction: "vertical",
                onEnd: () => persistCategoryOrder(categoryContainer)
            });
            state.sortableInstances.push(sortable);
        }

        state.root.querySelectorAll(selectors.reorderItemsContainer).forEach(container => {
            const sortable = new Sortable(container, {
                animation: 150,
                handle: ".menu-item-row",
                direction: "vertical",
                onEnd: () => persistItemOrder(container)
            });
            state.sortableInstances.push(sortable);
        });
    }

    function persistCategoryOrder(container) {
        const menuId = container.getAttribute("data-menu-id");
        if (!menuId) {
            return;
        }

        const categoryIds = Array.from(container.querySelectorAll("[data-category-id]"))
            .map(element => element.getAttribute("data-category-id"))
            .filter(Boolean);

        const url = buildUrl(`/MenuCategories/Reorder?menuId=${menuId}`);

        fetch(url, {
            method: "POST",
            headers: buildJsonHeaders(),
            body: JSON.stringify({ categoryIds })
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    replaceCategoryList(html);
                    initializeSortables();
                    showAlert("success", "ترتیب دسته‌ها به‌روزرسانی شد.");
                }
            })
            .catch(error => {
                console.error("Menu category reorder error", error);
                showAlert("danger", "در ذخیره ترتیب دسته‌ها خطایی رخ داد.");
            });
    }

    function persistItemOrder(container) {
        const menuId = container.getAttribute("data-menu-id");
        const categoryId = container.getAttribute("data-category-id");
        if (!menuId || !categoryId) {
            return;
        }

        const itemIds = Array.from(container.querySelectorAll("[data-item-id]"))
            .map(element => element.getAttribute("data-item-id"))
            .filter(Boolean);

        const url = buildUrl(`/MenuItems/Reorder?menuId=${menuId}&categoryId=${categoryId}`);

        fetch(url, {
            method: "POST",
            headers: buildJsonHeaders(),
            body: JSON.stringify({ itemIds })
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    replaceCategoryList(html);
                    initializeSortables();
                    showAlert("success", "ترتیب آیتم‌ها به‌روزرسانی شد.");
                }
            })
            .catch(error => {
                console.error("Menu item reorder error", error);
                showAlert("danger", "در ذخیره ترتیب آیتم‌ها خطایی رخ داد.");
            });
    }

    function buildHeaders() {
        const headers = new Headers();
        const token = getAntiForgeryToken();
        if (token) {
            headers.append("RequestVerificationToken", token);
        }
        return headers;
    }

    function buildJsonHeaders() {
        const headers = buildHeaders();
        headers.append("Content-Type", "application/json;charset=UTF-8");
        return headers;
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector(antiForgerySelector);
        return tokenInput ? tokenInput.value : null;
    }

    function setLoading(button, isLoading) {
        if (!button) {
            return;
        }

        if (isLoading) {
            button.setAttribute("disabled", "disabled");
            button.dataset.originalText = button.textContent || button.value || "";
            if (button.tagName === "BUTTON") {
                button.textContent = "در حال پردازش...";
            }
        } else {
            button.removeAttribute("disabled");
            if (button.dataset.originalText) {
                if (button.tagName === "BUTTON") {
                    button.textContent = button.dataset.originalText;
                }
                delete button.dataset.originalText;
            }
        }
    }

    function showAlert(type, message) {
        const container = document.querySelector(selectors.alerts);
        if (!container) {
            return;
        }

        container.innerHTML = `
            <div class="alert alert-${type}" role="alert">
                ${message}
            </div>`;
    }
})();
