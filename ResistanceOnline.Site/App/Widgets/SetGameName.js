define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        text: ko.observable(),
        set: function () {
            $.connection.gameHub.server.setGameName(viewModel.gameId(), viewModel.text());
            viewModel.text('');
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});