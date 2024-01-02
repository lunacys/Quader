use macroquad::prelude::*;

use crate::game_root::GameRoot;

mod game_root;
mod renderable;
mod board_controller;
mod updatable;
mod board_renderer;
mod board_controller_bot;
mod board_manager;

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