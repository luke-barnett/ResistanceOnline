define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        approve: ko.observable(),
        fail: function () {
            $.connection.gameHub.server.failQuest(viewModel.gameId());
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});