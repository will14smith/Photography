import * as React from "react";
import { Route, Switch } from "react-router";

function Home() {
  return <div>Hello World!</div>;
}
import Value from "./containers/Value";

const routes = (
  <Switch>
    <Route exact path="/" component={Home} />
    <Route path="/value" component={Value} />
    {/* <Route component={NotFOund} /> */}
  </Switch>
);

export default routes;
