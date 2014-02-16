define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        success: ko.observable(),
        submitQuestCard: function () {
            $.connection.gameHub.server.submitQuestCard(viewModel.gameId(), viewModel.success());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});