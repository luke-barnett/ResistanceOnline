define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel =  {
        games: data,
        //create game
        players: ko.observable(5),
        createGame: function () {
            $.connection.gameHub.server.createGame(viewModel.players());
        }
    };

    return viewModel;
});