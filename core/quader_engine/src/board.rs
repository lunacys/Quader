﻿use std::cell::{Ref, RefCell, RefMut};
use std::collections::{VecDeque};
use std::rc::Rc;
use rand::{Rng, SeedableRng};
use rand_chacha::ChaCha8Rng;
use serde::{Deserialize, Serialize};
use crate::board_command::{BoardCommand, BoardCommandType, BoardMoveDir};
use crate::cell_holder::{CellHolder};
use crate::game_settings::{GameSettings};
use crate::gravity_mgr::GravityMgr;
use crate::piece::{Piece, RotationDirection};
use crate::piece_generators::{PieceGeneratorBag7};
use crate::piece_mgr::PieceMgr;
use crate::primitives::Point;
use crate::replays::MoveInfo;
use crate::wall_kick_data::WallKickData;

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum GameState {

}

struct All {
    game_settings: GameSettings,

    // INNER LOGIC:
    test_queue: VecDeque<Vec<Point>>,
    cell_holder: CellHolder,
    current_piece: Piece,

    // SCORING:
    // ScoringMgr | combo: u32,
    // ScoringMgr | b2b: u32,
    last_move: MoveInfo,
    //replay: Replay,
    // DamageMgr | attack_queue: VecDeque<u32>,
    // DamageMgr | incoming_damage: Vec<u32>,
    // DamageMgr | last_garbage_x: u32,

    // UPDATABLE
    // GravityMgr | cur_gravity: f32,
    // GravityMgr | cur_lock: f32,
    // GravityMgr | intermediate_y: f32,
    // GravityMgr | y_needs_update: bool,
    // GravityMgr | y_to_check: u32,
    // DamageMgr | cur_garbage_cd: f32,
    // TimeMgr | cur_tick: f64,
}

pub struct Board {
    game_settings: GameSettings,
    /*gravity_mgr: GravityMgr<'a, 'a>,
    piece_mgr: PieceMgr<'a>,
    scoring_mgr: ScoringMgr,
    time_mgr: TimeMgr,
    damage_mgr: DamageMgr,
    cell_holder: CellHolder,
    rng_mgr: RngManager,
    damage_calculator: DamageCalculator<'a>,*/

    cell_holder: Rc<RefCell<CellHolder>>,
    piece_mgr: Rc<RefCell<PieceMgr>>,
    gravity_mgr: Rc<RefCell<GravityMgr>>,
    is_enabled: bool,
    seed: u64,
    piece_generator: PieceGeneratorBag7,
    components: Vec<Rc<RefCell<dyn BoardComponent>>>,
    last_garbage_x: i32,
    rng: ChaCha8Rng
}

impl Board {

    pub fn new(game_settings: GameSettings, seed: u64) -> Self {

        let cell_holder = Rc::new(RefCell::new(CellHolder::new(game_settings.get_board())));
        let piece_mgr =  Rc::new(RefCell::new(PieceMgr::new(&game_settings, Rc::clone(&cell_holder))));
        let gravity_mgr = Rc::new(RefCell::new(GravityMgr::new(game_settings.get_gravity(), Rc::clone(&piece_mgr))));

        Self {
            game_settings,
            cell_holder,
            piece_mgr,
            gravity_mgr,
            is_enabled: true,
            seed,
            piece_generator: PieceGeneratorBag7::new(1),
            components: vec![],
            last_garbage_x: -1,
            rng: SeedableRng::from_entropy()
        }
    }

    pub fn exec_cmd(&mut self, cmd: &BoardCommand) {
        match cmd.get_type() {
            BoardCommandType::Move(dir, delta) => {
                match dir {
                    BoardMoveDir::Left => self.move_left(*delta),
                    BoardMoveDir::Right => self.move_right(*delta)
                }
            },
            BoardCommandType::Rotate(dir) => self.rotate(&WallKickData::default(), dir),
            BoardCommandType::HardDrop => self.hard_drop(),
            BoardCommandType::SoftDrop(delta) => self.soft_drop(*delta),
            BoardCommandType::SendGarbage(amount, messiness) => self.send_garbage(*amount, *messiness),
            BoardCommandType::Update(dt) => self.update(*dt)
        }
    }

    pub fn update(&mut self, dt: f32) {
        self.gravity_mgr.borrow_mut().update(dt);
        self.piece_mgr.borrow_mut().update(dt);
    }

    pub fn move_left(&mut self, delta: i32) {
        self.piece_mgr.borrow_mut().move_left(delta);
    }

    pub fn move_right(&mut self, delta: i32) {
        self.piece_mgr.borrow_mut().move_right(delta);
    }

    pub fn rotate(&mut self, wkd: &WallKickData, direction: &RotationDirection) {
        self.piece_mgr.borrow_mut().rotate(wkd, direction);
        self.gravity_mgr.borrow_mut().prolong_lock();
    }

    pub fn hard_drop(&mut self) {
        self.piece_mgr.borrow_mut().hard_drop();
    }

    pub fn soft_drop(&mut self, delta: u32) {
        self.piece_mgr.borrow_mut().soft_drop(delta);
    }

    pub fn send_garbage(&mut self, amount: u32, _messiness: u32) {
        let mut ch = self.cell_holder.borrow_mut();

        let width = self.game_settings.get_board().width as u32;

        let garbage_hole_x: u32 = if self.last_garbage_x == -1 {
            // TODO: Use messiness
            self.rng.gen_range(0..width)
        } else {
            self.last_garbage_x as u32
        };

        for _ in 0..amount {
            ch.push_garbage(garbage_hole_x);
        }
    }

    pub fn get_cell_holder(&self) -> Ref<CellHolder> {
        self.cell_holder.borrow()
    }

    pub fn get_piece_mgr(&self) -> Ref<PieceMgr> {
        self.piece_mgr.borrow()
    }

    pub fn find_nearest_y(&self) -> u32 {
        self.get_piece_mgr().find_nearest_y()
    }

    pub(crate) fn get_piece_mgr_mut(&self) -> RefMut<PieceMgr> {
        self.piece_mgr.borrow_mut()
    }
}

pub trait BoardStateful {
    fn reset(&mut self);
    fn enable(&mut self) { }
    fn disable(&mut self) { }
    fn is_enabled(&self) -> bool;
}
impl BoardStateful for Board {
    fn reset(&mut self) {
        self.piece_mgr.borrow_mut().reset();
        self.gravity_mgr.borrow_mut().reset();
        self.cell_holder.borrow_mut().reset();
    }

    fn enable(&mut self) {
        if self.is_enabled {
            return;
        }

        self.piece_mgr.borrow_mut().enable();
        self.gravity_mgr.borrow_mut().enable();
        self.cell_holder.borrow_mut().enable();

        self.is_enabled = true;
    }

    fn disable(&mut self) {
        if !self.is_enabled {
            return;
        }

        self.piece_mgr.borrow_mut().disable();
        self.gravity_mgr.borrow_mut().disable();
        self.cell_holder.borrow_mut().disable();

        self.is_enabled = false;
    }

    fn is_enabled(&self) -> bool {
        self.is_enabled
    }
}

pub trait BoardComponent {
    fn get_name(&self) -> &'static str;
    fn reset(&mut self);
    fn enable(&mut self) { }
    fn disable(&mut self) { }
    fn update(&mut self, _dt: f32) { }
}


