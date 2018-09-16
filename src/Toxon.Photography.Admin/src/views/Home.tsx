import * as React from "react";

import { deploy } from "../api/deploy";
import ViewHeader from "../components/ViewHeader";

async function deploySite(event: React.MouseEvent<HTMLButtonElement>) {
  await deploy();
}

export default function Home() {
  return (
    <div>
      <ViewHeader title="Photography Admin" />
      <p>Click a link on the sidebar to start</p>
      <p>
        <button className="btn btn-danger" onClick={deploySite}>
          Deploy Site
        </button>
      </p>
    </div>
  );
}
