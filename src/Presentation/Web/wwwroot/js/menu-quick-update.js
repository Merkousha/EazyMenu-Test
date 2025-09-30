(function () {
    "use strict";

    const selectors = {
        root: "[data-quick-update]",
        saveButton: "[data-quick-save]",
        inventoryMode: "[data-inventory-mode]",
        inventoryQuantity: "[data-inventory-quantity]",
        inventoryThreshold: "[data-inventory-threshold]",
        alerts: "#menu-quick-alerts",
        wrapper: "#menu-quick-update-wrapper"
    };

    const antiForgerySelector = "#menu-anti-forgery input[name='__RequestVerificationToken']";

    const state = {
        root: null,
        menuId: null,
        baseUrl: ""
    };

    document.addEventListener("DOMContentLoaded", initialize);

    function initialize() {
        state.root = document.querySelector(selectors.root);
        if (!state.root) {
            return;
        }

        state.menuId = state.root.getAttribute("data-menu-id");
        state.baseUrl = state.root.getAttribute("data-base-url") || "";

        state.root.addEventListener("click", onRootClick);
        state.root.addEventListener("change", onRootChange);

        refreshInventoryControls();
    }

    function onRootClick(event) {
        const button = event.target.closest(selectors.saveButton);
        if (!button) {
            return;
        }

        const row = button.closest("[data-quick-item]");
        if (!row) {
            return;
        }

        submitRow(row, button);
    }

    function onRootChange(event) {
        const select = event.target.closest(selectors.inventoryMode);
        if (!select) {
            return;
        }

        const container = select.closest("[data-inventory-root]");
        toggleInventoryInputs(container, select.value);
    }

    function refreshInventoryControls() {
        state.root.querySelectorAll(selectors.inventoryMode).forEach(select => {
            const container = select.closest("[data-inventory-root]");
            toggleInventoryInputs(container, select.value);
        });
    }

    function toggleInventoryInputs(container, mode) {
        if (!container) {
            return;
        }

        const quantity = container.querySelector(selectors.inventoryQuantity);
        const threshold = container.querySelector(selectors.inventoryThreshold);
        const track = String(mode).toLowerCase() === "track";

        if (quantity) {
            quantity.disabled = !track;
            if (!track) {
                quantity.value = "";
            }
        }

        if (threshold) {
            threshold.disabled = !track;
            if (!track) {
                threshold.value = "";
            }
        }
    }

    function submitRow(row, button) {
        const categoryId = row.getAttribute("data-category-id");
        const itemId = row.getAttribute("data-item-id");
        if (!categoryId || !itemId || !state.menuId) {
            console.warn("Quick update missing identifiers");
            return;
        }

        const payload = buildPayload(row);
        if (!payload) {
            showAlert("danger", "مقادیر وارد شده معتبر نیست." );
            return;
        }

        setLoading(button, true);

        const url = buildUrl(`/MenuItems/QuickUpdate?menuId=${state.menuId}&categoryId=${categoryId}&itemId=${itemId}`);

        fetch(url, {
            method: "POST",
            headers: buildJsonHeaders(),
            body: JSON.stringify(payload)
        })
            .then(response => handlePartialResponse(response))
            .then(html => {
                if (html) {
                    updateTable(html);
                    showAlert("success", "آیتم با موفقیت به‌روزرسانی شد.");
                }
            })
            .catch(error => {
                console.error("Quick update error", error);
                showAlert("danger", "در ذخیره تغییرات خطایی رخ داد.");
            })
            .finally(() => setLoading(button, false));
    }

    function buildPayload(row) {
        const basePriceInput = row.querySelector("[data-field='basePrice']");
        if (!basePriceInput) {
            return null;
        }

        const basePrice = parseFloat(basePriceInput.value ?? "0");
        if (Number.isNaN(basePrice) || basePrice < 0) {
            return null;
        }

        const currency = row.getAttribute("data-currency") || "IRT";

        const channelInputs = row.querySelectorAll("input[data-channel]");
        const channelPrices = {};
        channelInputs.forEach(input => {
            const value = input.value?.trim();
            const channel = input.getAttribute("data-channel");
            if (!channel) {
                return;
            }

            if (value && !Number.isNaN(Number(value))) {
                channelPrices[channel] = Number(value);
            }
        });

        const inventoryMode = row.querySelector(selectors.inventoryMode)?.value || "Infinite";
        const quantityInput = row.querySelector(selectors.inventoryQuantity);
        const thresholdInput = row.querySelector(selectors.inventoryThreshold);

        const quantityValue = quantityInput && !quantityInput.disabled ? quantityInput.value : null;
        const thresholdValue = thresholdInput && !thresholdInput.disabled ? thresholdInput.value : null;

        const payload = {
            basePrice,
            currency,
            channelPrices: Object.keys(channelPrices).length > 0 ? channelPrices : null,
            inventory: {
                mode: inventoryMode,
                quantity: quantityValue !== null && quantityValue !== "" ? Number(quantityValue) : null,
                threshold: thresholdValue !== null && thresholdValue !== "" ? Number(thresholdValue) : null
            },
            isAvailable: row.querySelector("[data-field='isAvailable']")?.checked ?? false
        };

        return payload;
    }

    function updateTable(html) {
        const wrapper = document.querySelector(selectors.wrapper);
        if (!wrapper) {
            return;
        }

        wrapper.innerHTML = html;
        refreshInventoryControls();
    }

    function buildUrl(path) {
        if (path.startsWith("http")) {
            return path;
        }

        return `${state.baseUrl}${path}`;
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
            button.disabled = true;
            button.dataset.originalText = button.textContent || "";
            button.textContent = "در حال ذخیره...";
        } else {
            button.disabled = false;
            if (button.dataset.originalText) {
                button.textContent = button.dataset.originalText;
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
