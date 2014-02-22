define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        player: ko.observable(),
        players: ko.observableArray(),
        assignExcalibur: function () {
            $.connection.gameHub.server.assignExcalibur(viewModel.gameId(), viewModel.player().Text());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.players(game.AssignExcaliburSelectList());
        }
    };

    return viewModel;
});