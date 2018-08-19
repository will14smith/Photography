import * as React from "react";
import * as Icon from "react-feather";
import { Photograph } from "../api/photograph";

export interface Props {
  getPhotographs: () => void;

  loading: boolean;
  error?: Error;
  photographs: Photograph[];
}

export default class Home extends React.Component<Props> {
  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotographs();
  }

  public render() {
    return (
      <div>
        <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
          <h1 className="h2">Hello World!</h1>
          <div className="btn-toolbar mb-2 mb-md-0">
            <div className="btn-group mr-2">
              <button className="btn btn-sm btn-outline-secondary">
                <Icon.Plus />
              </button>
            </div>
          </div>
        </div>

        <ul>
          {this.props.loading && <li>Loading...</li>}
          {this.props.error && (
            <li>
              <strong>Error: </strong>
              {this.props.error}
            </li>
          )}
          {!this.props.loading &&
            this.props.photographs.map(photograph => (
              <li>
                {photograph.Id} - {photograph.Title}
              </li>
            ))}
        </ul>
      </div>
    );
  }
}
