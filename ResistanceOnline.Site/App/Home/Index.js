<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
﻿define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel =  {
        games: data,

        //create game
        impersonation: ko.observable('false'),
        players: ko.observable(5),
        createGame: function () {
            $.connection.gameHub.server.createGame(viewModel.players(), viewModel.impersonation());
            //todo redirect?
        }
    };

    return viewModel;
=======
﻿define(function (require) {
    return {};
>>>>>>> add signalr/durandal
=======
﻿define(['data', 'knockout'], function (data, ko) {
=======
﻿define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

>>>>>>> more knockout
    return { games: data };
>>>>>>> knockouts
});