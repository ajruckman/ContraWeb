window.initChart = function (dataSerialized, actionsSerialized) {
    const inputData = JSON.parse(dataSerialized);
    console.log(inputData);

    const actions = JSON.parse(actionsSerialized);

    let data = [];
    let labels = [];

    for (let hour in inputData) {
        data.push(inputData[hour]);
    }

    console.log(labels);
    console.log(data[data.length - 1]);

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

    const valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.title.text = "Count";
    // valueAxis.tooltip.disabled = true;

    dateAxis.title.text = "Hour";
    /*
        dateAxis.baseInterval = {
            "timeUnit": "hour",
            "count": 1,
        };
    */

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.xAxis = dateAxis;
    chart.scrollbarX = new am4core.Scrollbar();

    for (let action of actions) {
        console.log('-> ' + action);

        let series = chart.series.push(new am4charts.LineSeries());

        series.name = action;
        series.dataFields.dateX = "time";
        series.dataFields.valueY = action;

        // series.sequencedInterpolation = true;

        // series.strokeWidth = 2;
        series.fillOpacity = 0.3;
        series.strokeOpacity = 0.3;
        series.stacked = true;

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
};
