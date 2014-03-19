define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        players: ko.observableArray(),
        assignExcalibur: function (player) {
            $.connection.gameHub.server.assignExcalibur(viewModel.gameId(), player);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.players(game.AssignExcaliburSelectList());
        }
    };

    return viewModel;
});