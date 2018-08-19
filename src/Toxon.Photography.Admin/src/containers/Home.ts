import { connect } from "react-redux";

import Home from "../components/Home";

import * as redux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

export function mapStateToProps({ photographs }: RootState) {
  const all = photographs.ids.map(id => photographs.byId[id]);

  return {
    error: photographs.error,
    loading: photographs.loading,
    photographs: all
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotographs: () => dispatch(redux.getPhotographs())
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Home);
