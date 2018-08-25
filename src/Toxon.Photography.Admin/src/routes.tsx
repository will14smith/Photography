import * as React from "react";
import { Route, Switch } from "react-router";

import Home from "./views/Home";
import NotFound from "./views/NotFound";
import PhotographCreate from "./views/PhotographCreate";
import PhotographList from "./views/PhotographList";

const routes = (
  <Switch>
    <Route exact path="/" component={Home} />
    <Route path="/photographs/create" component={PhotographCreate} />
    <Route path="/photographs" component={PhotographList} />
    <Route component={NotFound} />
  </Switch>
);

export default routes;
