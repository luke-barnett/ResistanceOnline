define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    console.log('not u');
    //todo ctor?
    var viewModel = {
        gameId: ko.observable(),
        name: ko.observable(''),
        joinGame: function () {
            $.connection.gameHub.server.joinGame(viewModel.gameId(), viewModel.name());
            //player guid?
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});