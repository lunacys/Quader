/*use std::sync::Arc;
use cold_clear::{BotPollState, Info, Interface};
use quader_engine::board::Board;
use quader_engine::cell_holder::BoolArray;
use quader_engine::game_settings::GameSettings;
use quader_engine::piece::{PieceType, RotationDirection};
use quader_engine::piece_mgr::UpdateErrorReason;
use quader_engine::replays::MoveResult;
use quader_engine::wall_kick_data::WallKickData;
use crate::{BotSettings, piece_type_to_piece};


pub struct BotBoard {
    pub engine_board: Board,
    pub game_settings: GameSettings,
    pub bot_interface: Box<Interface>,
    pub bot_settings: BotSettings,
    elapsed_secs: f32,
    hold_used: bool,
    pub is_enabled: bool
}

fn create_bot_interface(board: &Board) -> Box<Interface> {
    let mut bot_board = libtetris::Board::new();
    bot_board.add_next_piece(piece_type_to_piece(board.piece_mgr.cur_piece.get_type()));
    for pt in &board.piece_mgr.piece_queue.queue {
        bot_board.add_next_piece(piece_type_to_piece(*pt));
    }

    Box::new(Interface::launch(
        bot_board,
        cold_clear::Options::default(),
        cold_clear::evaluation::Standard::default(),
        None
    ))
}

impl BotBoard {
    pub fn new(game_settings: GameSettings, wkd: Arc<WallKickData>, seed: u64, bot_settings: BotSettings) -> Self {

        let board = Board::new(game_settings, wkd, seed);

        let bot_interface = create_bot_interface(&board);

        Self {
            engine_board: board,
            bot_interface,
            game_settings,
            bot_settings,
            elapsed_secs: 0.0,
            hold_used: false,
            is_enabled: true
        }
    }

    pub fn update(&mut self, dt: f32) -> Option<Result<MoveResult, UpdateErrorReason>> {
        if !self.is_enabled {
            return None;
        }

        self.elapsed_secs += dt;

        if self.bot_settings.target_pps <= 0.0 {
            return Some(self.do_bot_move());
        } else if self.elapsed_secs >= 1.0 / self.bot_settings.target_pps {
            self.elapsed_secs = 0.0;
            return Some(self.do_bot_move());
        }

        if let Some(r) = self.engine_board.update(dt) {
            return Some(r);
        }

        None
    }

    pub fn reset(&mut self, new_seed: Option<u64>) {
        self.engine_board.reset(new_seed);
        self.hold_used = false;
        self.elapsed_secs = 0.0;
        self.is_enabled = true;

        self.bot_interface = create_bot_interface(&self.engine_board);
    }

    pub fn add_next_piece(&self, piece_type: PieceType) {
        self.bot_interface.add_next_piece(piece_type_to_piece(piece_type));
    }

    pub fn request_next_move(&self, incoming_garbage: u32) {
        self.bot_interface.suggest_next_move(incoming_garbage);
    }

    pub fn poll_next_move(&self) -> Result<(libtetris::Move, Info), BotPollState> {
        self.bot_interface.poll_next_move()
    }

    pub async fn block_next_move(&self) -> Option<(libtetris::Move, Info)> {
        self.bot_interface.block_next_move().await
    }

    pub fn play_next_move(&self, falling_piece: libtetris::FallingPiece) {
        self.bot_interface.play_next_move(falling_piece);
    }

    fn do_bot_move(&mut self) -> Result<MoveResult, UpdateErrorReason> {
        let incoming_garbage = self.engine_board.garbage_mgr.queue
            .iter()
            .map(|q| q.amount)
            .fold(0, |acc, q| acc + q);

        self.request_next_move(incoming_garbage as u32);

        let res = match self.block_next_move() {
            Some((m, _info)) => {
                self.play_next_move(m.expected_location);
                //let _plan = info.plan();

                if m.hold {
                    let _ = self.engine_board.hold_piece();
                    if !self.hold_used {
                        self.bot_interface.add_next_piece(
                            piece_type_to_piece(
                                *self.engine_board.piece_mgr.piece_queue.queue.back().unwrap()
                            )
                        );
                        self.hold_used = true;
                    }
                }

                for input in m.inputs.iter() {
                    self.exec_input(input);
                }

                match self.engine_board.hard_drop() {
                    Ok(hd) => {
                        if !hd.attack.in_damage_queue.is_empty() {
                            // update bot's board
                            let new_board = self.engine_board.piece_mgr.cell_holder.to_bool_array();
                            let mut field = [[false; 10]; 40];

                            for (y, row) in new_board.iter().enumerate() {
                                for (x, val) in row.iter().enumerate() {
                                    field[39 - y][x] = *val;
                                }
                            }

                            self.bot_interface.reset(field, hd.b2b > 0, hd.combo);
                        }

                        Ok(hd)
                    }
                    Err(err) => { Err(err) }
                }

            }
            None => {
                Err(UpdateErrorReason::BoardDead)
            }
        };

        self.bot_interface.add_next_piece(
            piece_type_to_piece(
                *self.engine_board.piece_mgr.piece_queue.queue.back().unwrap()
            )
        );

        res
    }

    fn exec_input(&mut self, input: &libtetris::PieceMovement) {
        match input {
            libtetris::PieceMovement::Left => { self.engine_board.move_left(1); },
            libtetris::PieceMovement::Right => { self.engine_board.move_right(1); },
            libtetris::PieceMovement::Cw => { self.engine_board.rotate(RotationDirection::Clockwise); },
            libtetris::PieceMovement::Ccw => { self.engine_board.rotate(RotationDirection::CounterClockwise); },
            libtetris::PieceMovement::SonicDrop => { self.engine_board.soft_drop(40); },
        };
    }
}*/