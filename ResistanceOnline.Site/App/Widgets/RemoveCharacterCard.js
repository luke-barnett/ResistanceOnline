define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        card: ko.observable(),
        cards: ko.observableArray(),
        removeCharacterCard: function () {
            $.connection.gameHub.server.removeCharacterCard(viewModel.gameId(), viewModel.card().Text());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.cards(game.RemoveCharacterCardSelectList());
        }
    };

    return viewModel;
});