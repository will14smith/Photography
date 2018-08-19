import * as React from "react";
import { Link, Route, Switch } from "react-router-dom";

import Value from "./containers/Value";

function Home() {
  return <div>Hello World!</div>;
}

export default function() {
  return (
    <div>
      <ul>
        <li>
          <Link to="/">Home</Link>
        </li>
        <li>
          <Link to="/value">Value</Link>
        </li>
      </ul>
      <Switch>
        <Route exact={true} path="/" component={Home} />
        <Route path="/value" component={Value} />
      </Switch>
    </div>
  );
}
