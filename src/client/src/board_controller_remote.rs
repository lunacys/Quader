/*
 * Copyright (c) Grigory Alfyorov. Licensed under the MIT License.
 * See the LICENSE file in the repository root for full licence text.
 */

use std::net::TcpStream;

use std::sync::mpsc;
use std::thread;
use tungstenite::{connect, stream::MaybeTlsStream, Message, WebSocket};
use url::Url;

pub struct BoardControllerRemote {
    socket: WebSocket<MaybeTlsStream<TcpStream>>,
}

impl BoardControllerRemote {
    pub fn new(connection_uri: &str) -> Self {
        let (mut socket, response) =
            connect(Url::parse(connection_uri).unwrap()).expect("Can't connect");

        println!("Connected to the server");
        println!("Response HTTP code: {}", response.status());
        println!("Response contains the following headers:");

        for (ref header, _value) in response.headers() {
            println!("* {}", header);
        }

        socket.send(Message::Text("Hello WS!".into())).unwrap();
        loop {
            let msg = socket.read().expect("Error reading message");
            println!("Received: {}", msg);

            match msg {
                Message::Text(content) => {
                    //serde_json::from_str(&content);
                }
                Message::Close(_close_frame) => {
                    break;
                }
                Message::Ping(_) | Message::Pong(_) | Message::Frame(_) | Message::Binary(_) => {}
            }

            break;
        }

        Self { socket }
    }

    pub fn close(&mut self) {
        self.socket.close(None).ok();
    }
}
