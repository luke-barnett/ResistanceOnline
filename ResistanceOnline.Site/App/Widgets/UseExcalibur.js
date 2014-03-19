define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        players: ko.observableArray(),
        useExcalibur: function (player) {
            $.connection.gameHub.server.useExcalibur(viewModel.gameId(), player);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.players(game.UseExcaliburSelectList());
        }
    };

    return viewModel;
});