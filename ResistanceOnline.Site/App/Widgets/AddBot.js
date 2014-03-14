define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        name: ko.observable(),
        addBot: function () {
            $.connection.gameHub.server.addBot(viewModel.gameId(), name);
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});