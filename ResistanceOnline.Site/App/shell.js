define(['require', 'plugins/router', 'signalr.hubs'], function (require, router, signalr) {
    
    var update = function (games) {
        console.log(games);
    };

    var gameHub = $.connection.gameHub;
    gameHub.on('update', update);
    $.connection.hub.start();
        
    return {
        router: router,
        activate: function () {
            router.map([
              { route: '', title: 'Home', moduleId: 'home/index', nav: true },
              { route: 'game/:gameId', title: 'Game', moduleId: 'game/index', nav: true },
            ]).buildNavigationModel();

            return router.activate();
        }
    };
});