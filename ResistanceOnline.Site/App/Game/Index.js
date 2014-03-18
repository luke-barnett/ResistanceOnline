define(['data', 'knockout', 'knockout.punches', 'bootstrap'], function (data, ko, kop) {
    ko.punches.enableAll();
    var deferred = new $.Deferred();
    data.subscribe(function () { deferred.resolve() });
    var viewModel = {
    	gameid: ko.observable(0),
    	game: ko.computed(function ()
    	{
    		return $.grep(data(), function (item)
    		{
    			return item.GameId() == viewModel.gameid();
    		})[0]
    	}, this, { deferEvaluation: true })
    };
    viewModel.activate = function (gameid) {
    	viewModel.gameid(gameid);
    	if (!data().length)
    		return deferred;
    };

    return viewModel;
});