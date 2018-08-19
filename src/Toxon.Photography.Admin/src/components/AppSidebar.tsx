import * as React from "react";
import * as Icon from "react-feather";
import { NavLink } from "react-router-dom";

import "./AppSidebar.css";

export interface Props {
  currentPathName?: string;
}

const AppSidebar = ({ currentPathName }: Props) => {
  return (
    <div className="sidebar-sticky">
      <ul className="nav flex-column">
        <li className="nav-item">
          <NavLink className="nav-link" exact to="/">
            <Icon.Home size="16" /> Dashboard
          </NavLink>
        </li>

        <li className="nav-item">
          <NavLink className="nav-link" to="/value">
            <Icon.Hash size="16" /> Value
          </NavLink>
        </li>
      </ul>
    </div>
  );
};

export default AppSidebar;
