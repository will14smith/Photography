import * as React from "react";

import { Storage } from "aws-amplify";

export interface Props extends React.HTMLAttributes<HTMLElement> {
  imageKey: string;
}

interface State {
  loading: boolean;
  src?: string;
}

export default class S3Image extends React.Component<Props, State> {
  public state: State = {
    loading: true
  };

  public componentDidMount() {
    this.loadImage();
  }

  public componentDidUpdate(prevProps: Props) {
    if (prevProps.imageKey !== this.props.imageKey) {
      this.loadImage();
    }
  }

  public render() {
    const { loading, src } = this.state;
    const { imageKey, ...props } = this.props;

    return loading ? (
      <div {...props}>Loading...</div>
    ) : (
      <img {...props} src={src} />
    );
  }

  private async loadImage() {
    this.setState({ loading: true });

    const result = await Storage.get(this.props.imageKey, {
      customPrefix: { public: "" },
      level: "public"
    });

    this.setState({ loading: false, src: result as string });
  }
}
