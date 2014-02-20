define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
	var viewModel = {
		gameId: ko.observable(),
		rules: ko.observableArray([]),
		rule: ko.observable(),
		addRule: function () {
			$.connection.gameHub.server.addRule(viewModel.gameId(), viewModel.rule().Value());
		},
		activate: function (game) {
			viewModel.gameId(game.GameId());
			viewModel.rules(game.AllRulesSelectList());
		}
	};

	return viewModel;
});