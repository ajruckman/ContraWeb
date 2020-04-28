window.initLogActionsPerHourChart = function (dataSerialized, actionsSerialized) {
    try {
        const inputData = JSON.parse(dataSerialized);

        const actions = JSON.parse(actionsSerialized);

        let data = [];
        let labels = [];

        console.log(inputData);
        
        for (let hour in inputData) {
            data.push(inputData[hour]);
        }

        //

        const theme = localStorage.getItem('UISet_Theme');
        if (theme === 'Dark' || theme === 'Matrix') {
            am4core.useTheme(am4themes_dark);
        }

        const chart = am4core.create("Stats_LogActionsPerHour", am4charts.XYChart);

        chart.data = data;
        chart.dateFormatter.inputDateFormat = "yyyy-MM-ddTHH:mm:ss";
        chart.paddingRight = 20;

        const dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        // dateAxis.tooltip.color = am4core.color("#F00");
        dateAxis.renderer.labels.template.opacity = 0.75;

        const valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.title.text = "Count";
        valueAxis.renderer.labels.template.opacity = 0.75;
        // valueAxis.tooltip.disabled = true;

        dateAxis.title.text = "Hour";
        // /*
            dateAxis.baseInterval = {
                "timeUnit": "minute",
                "count": 1,
            };
        // */

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.xAxis = dateAxis;
        chart.scrollbarX = new am4core.Scrollbar();

        for (let action of actions) {
            let series = chart.series.push(new am4charts.LineSeries());

            series.name = action;
            series.dataFields.dateX = "time";
            series.dataFields.valueY = action;

            // series.sequencedInterpolation = true;

            // series.strokeWidth = 2;
            series.fillOpacity = 0.4;
            series.strokeOpacity = 0.3;
            series.stacked = true;
            
            // series.connect = false;

            // series.tooltipText = "[#FFF]" + action + ": {valueY.value}[/]";
            series.tooltipText = action + ": {valueY.value}";
            series.tooltip.getStrokeFromObject = true;
            series.tooltip.background.opacity = 0.8;
            series.tooltip.autoTextColor = false;

            if (action.startsWith('pass')) {
                series.stroke = am4core.color("#0F0");
                series.fill = am4core.color("#0F0");
                series.tooltip.label.fill = am4core.color("#000");

            } else if (action.startsWith('respond')) {
                series.stroke = am4core.color("#00F");
                series.fill = am4core.color("#00F");
                series.tooltip.label.fill = am4core.color("#FFF");

            } else if (action.startsWith('block')) {
                series.stroke = am4core.color("#F00");
                series.fill = am4core.color("#F00");
                series.tooltip.label.fill = am4core.color("#000");
            }
        }

        // console.log("-------")
        // document.getElementById("Stats_LogActionsPerHour_LoadingIndicator").display = "none";

    } catch (e) {
        console.log("window.initLogActionsPerHourChart uncaught exception:");
        console.log(e);
    }
};

window.initLogActionCountsChart = function (dataSerialized) {
    try {
        const inputData = JSON.parse(dataSerialized);

        let data = [];

        for (let action in inputData) {
            if (!inputData.hasOwnProperty(action)) {
                continue;
            }

            let color = "";

            if (action.startsWith('pass')) {
                color = am4core.color("#0F0");
            } else if (action.startsWith('respond')) {
                color = am4core.color("#00F");
            } else if (action.startsWith('block')) {
                color = am4core.color("#F00");
            }

            data.push({
                action: action,
                count: inputData[action],
                color: color,
            });
        }

        const chart = am4core.create("Stats_LogActionCounts", am4charts.PieChart);

        chart.data = data;

        const pieSeries = chart.series.push(new am4charts.PieSeries());
        pieSeries.dataFields.value = "count";
        pieSeries.dataFields.category = "action";

        pieSeries.fillOpacity = 0.75;
        pieSeries.slices.template.fillOpacity = 0.3;
        pieSeries.slices.template.propertyFields.fill = "color";

        pieSeries.tooltip.background.opacity = 0.8;
        pieSeries.tooltip.getStrokeFromObject = true;
    } catch (e) {
        console.log("window.initLogActionCountsChart uncaught exception:");
        console.log(e);
    }
};


window.initLeaseVendorCounts = function (dataSerialized) {
    try {
        const inputData = JSON.parse(dataSerialized);
        console.log(inputData);

        // let data = [];

        // for (let vendor in inputData) {
        //     if (!inputData.hasOwnProperty(vendor)) {
        //         continue;
        //     }
        //
        //     console.log(inputData[vendor]);
        //
        //     data.push({
        //         vendor: inputData[vendor].Item1,
        //         c: inputData[vendor].Item2,
        //         percent: inputData[vendor].Item3,
        //     });
        // }

        const chart = am4core.create("Stats_LeaseVendorCounts", am4charts.PieChart);
        
        chart.data = JSON.parse(dataSerialized);

        chart.chartContainer.minHeight = 20;
        chart.chartContainer.minWidth = 20;

        const pieSeries = chart.series.push(new am4charts.PieSeries());
        pieSeries.dataFields.value = "c";
        pieSeries.dataFields.category = "vendor";
        
        // pieSeries.labels.template.fontSize = 10;
        pieSeries.labels.template.text = "{category}: {value.percent.formatNumber('#.0')}%";
        
        pieSeries.fillOpacity = 0.75;
        pieSeries.slices.template.fillOpacity = 0.3;

        pieSeries.tooltip.background.opacity = 0.8;
        pieSeries.tooltip.getStrokeFromObject = true;
    } catch (e) {
        console.log("window.initLeaseVendorCounts uncaught exception:");
        console.log(e);
    }
};
