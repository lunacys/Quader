/*
 * Copyright (c) Grigory Alfyorov. Licensed under the MIT License.
 * See the LICENSE file in the repository root for full licence text.
 */

use macroquad::miniquad::log;
use macroquad::prelude::*;

use crate::game_root::GameRoot;

mod assets;
mod board_controller;
mod board_controller_bot;
mod board_controller_remote;
mod board_manager;
mod board_renderer;
mod entity;
mod game_root;
mod scene;
mod state_machine;

fn window_conf() -> Conf {
    Conf {
        window_title: "Quader".to_owned(),
        fullscreen: false,
        platform: miniquad::conf::Platform {
            linux_backend: miniquad::conf::LinuxBackend::WaylandWithX11Fallback,
            ..Default::default()
        },
        window_width: 1920,
        window_height: 1080,
        window_resizable: true,
        ..Default::default()
    }
}

#[macroquad::main(window_conf)]
async fn main() {
    // debug!("This is a debug message");

    let mut game = GameRoot::new();
    game.run().await
}
