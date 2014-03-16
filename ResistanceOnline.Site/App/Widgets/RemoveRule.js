define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        rules: ko.observableArray(),
        removeRule: function (rule) {
            $.connection.gameHub.server.removeRule(viewModel.gameId(), rule);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.rules(game.RemoveRuleSelectList());
        }
    };

    return viewModel;
});