use serde::{Deserialize, Serialize};
use crate::board::GameState;
use crate::piece::{PieceType, RotationDirection};

#[derive(Debug, Clone, Copy, Eq, PartialEq, Serialize, Deserialize)]
pub enum BoardMoveDir {
    Left, Right
}

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum BoardCommand {
    // move direction, delta
    Move(BoardMoveDir, i32),
    Rotate(RotationDirection),
    HardDrop,
    // delta
    SoftDrop(u32),
    // amount, messiness
    SendGarbage(u32, u32),
    // delta
    Update(f32),
    HoldPiece,
    RequestBoardLayout,

}

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub enum BoardMessage {
    NewPieceInQueue(PieceType),
    PieceUpdated,
    // Amount, hole x position
    GarbageReceived(u32, u32),
    GameStateChanged(GameState),
    PlayerRemoved,
    BoardUpdated
}