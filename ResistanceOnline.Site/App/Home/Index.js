define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    return { games: data };
});