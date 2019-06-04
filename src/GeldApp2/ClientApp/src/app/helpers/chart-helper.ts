import * as CanvasJS from 'src/libs/canvasjs.min';

export class ChartHelper {
    public static createChartId(): string {
        return 'chart-xxxxxxxxxxx'.replace(/[x]/g, function (c) {
            const r = 97 + Math.random() * 26;
            return String.fromCharCode(r);
        });
    }

    public static mapToDateItems(items: {x: any, y: any}[], invert = false): any[] {
        return items.map((i) => ({ x: new Date(i.x), y: invert ? -i.y : i.y }));
    }

    public static init() {
        CanvasJS.addCultureInfo('de',
            {
                decimalSeparator: ',',
                digitGroupSeparator: '.',
            });
    }
}
