import * as React from "react";
import { Route, Switch } from "react-router";

import Home from "./views/Home";
import NotFound from "./views/NotFound";
import PhotographList from "./views/PhotographList";

const routes = (
  <Switch>
    <Route exact path="/" component={Home} />
    <Route path="/photographs" component={PhotographList} />
    {/* <Route path="/photographs/create" component={PhotographCreate} /> */}
    <Route component={NotFound} />
  </Switch>
);

export default routes;
