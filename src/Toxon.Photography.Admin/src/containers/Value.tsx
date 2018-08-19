import { connect } from "react-redux";
import { Dispatch } from "redux";

import Value from "../components/Value";

import * as actions from "../redux/actions";
import { ValueState } from "../redux/store";

export function mapStateToProps({ value }: ValueState) {
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
