import { connect } from "react-redux";
import { Dispatch } from "redux";

import Value from "../components/Value";

import * as actions from "../redux/actions";
import { RootState } from "../redux/store";

export function mapStateToProps({ value }: RootState) {
  return {
    value
  };
}

export function mapDispatchToProps(dispatch: Dispatch<actions.ValueAction>) {
  return {
    onDecrement: () => dispatch(actions.decrementValue()),
    onIncrement: () => dispatch(actions.incrementValue())
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Value);
