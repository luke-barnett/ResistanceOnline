define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel = {
        games: data,

        //create game
        createGame: function () {
            $.connection.gameHub.server.createGame();
            //todo redirect?
        },

        //join game
        joinGame: function (game) {
            var id = game.children[0].value; //todo how does knockout work?
            $.connection.gameHub.server.joinGame(id);
            //todo redirect?        
        }
    }

    return viewModel;
});