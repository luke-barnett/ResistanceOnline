define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        text: ko.observable(),
        message: function () {
            $.connection.gameHub.server.message(viewModel.gameId(), viewModel.text());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});