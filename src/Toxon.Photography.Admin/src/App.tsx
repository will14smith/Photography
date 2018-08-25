import * as React from "react";

import { Auth } from "aws-amplify";
import { ConnectedRouter } from "connected-react-router";
import { History } from "history";

import AppHeader from "./components/AppHeader";
import AppSidebar from "./components/AppSidebar";
import routes from "./routes";

import "./App.css";

interface AppProps {
  // these are injected by aws-amplify
  authData?: any;
  authState?: "signIn" | "signedIn";

  history: History;
}

function logout() {
  return Auth.signOut();
}

const App = ({ history, authData, authState }: AppProps) => {
  return authState === "signedIn" ? (
    <ConnectedRouter history={history}>
      <div>
        <AppHeader />

        <div className="container-fluid">
          <div className="row">
            <nav className="col-md-2 d-none d-md-block bg-light sidebar">
              <AppSidebar username={authData.username} onLogout={logout} />
            </nav>

            <main role="main" className="col-md-9 ml-sm-auto col-lg-10 px-4">
              {routes}
            </main>
          </div>
        </div>
      </div>
    </ConnectedRouter>
  ) : null;
};

export default App;
