window.flatpickrInstances = {};

window.flatpickrInterop = {
    init: function (elementId, dotNetHelper, options) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error('Flatpickr element not found:', elementId);
            return;
        }

        const config = {
            dateFormat: options.dateFormat || 'Y/m/d',
            locale: options.locale || 'zh_tw',
            allowInput: false,
            clickOpens: true,
            onChange: function (selectedDates, dateStr, instance) {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnDateChanged', dateStr);
                }
            }
        };

        // 如果有初始日期，設定它
        if (options.defaultDate) {
            config.defaultDate = options.defaultDate;
        }

        // 建立 Flatpickr 實例
        const fp = flatpickr(element, config);
        window.flatpickrInstances[elementId] = fp;

        return true;
    },

    setDate: function (elementId, dateString) {
        const fp = window.flatpickrInstances[elementId];
        if (fp) {
            fp.setDate(dateString, false);
        }
    },

    destroy: function (elementId) {
        const fp = window.flatpickrInstances[elementId];
        if (fp) {
            fp.destroy();
            delete window.flatpickrInstances[elementId];
        }
    }
};
