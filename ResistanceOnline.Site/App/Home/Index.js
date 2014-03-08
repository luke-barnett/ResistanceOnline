define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel = {
        games: data,

        //create game
        createGame: function () {
            $.connection.gameHub.server.createGame();
            //todo redirect?
        },
    }

    return viewModel;
});