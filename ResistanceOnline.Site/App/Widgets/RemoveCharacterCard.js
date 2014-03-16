define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        cards: ko.observableArray(),
        removeCharacterCard: function (card) {
            $.connection.gameHub.server.removeCharacterCard(viewModel.gameId(), card);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.cards(game.RemoveCharacterCardSelectList());
        }
    };

    return viewModel;
});