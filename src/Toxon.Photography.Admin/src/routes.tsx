import * as React from "react";
import { Route, Switch } from "react-router";

import Home from "./components/Home";
import Value from "./containers/Value";

const routes = (
  <Switch>
    <Route exact path="/" component={Home} />
    <Route path="/value" component={Value} />
    {/* <Route component={NotFOund} /> */}
  </Switch>
);

export default routes;
