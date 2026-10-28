import { components } from "../contracts/emulator-api.g";
import { Req } from "../utils/types/Req";
import { Hamlet } from "./Hamlet-Act-1-Scene-2";

export function GetUpdateMocks() {
    const characterNames = [] as string[];
    const getUserId = (characterName: string): number => {
        const result = characterNames.indexOf(characterName);
        if (result >= 0) {
            return result + 1;
        }
        characterNames.push(characterName);
        return characterNames.length;
    }

    const updates = [] as Req<components["schemas"]["Update"]>[];
    let offset = 0;
    for (const record of Hamlet()) {
        if (record.type === "speech") {
        updates.push({
            update_id: offset,
                message: {
                    message_id: offset,
                    from: {
                        id: getUserId(record.character!),
                        username: record.character,
                        is_bot: record.character !== "Hamlet",
                    },
                    text: record.speech,
                }
            } as Req<components["schemas"]["Update"]>);
        } else if (record.type === "reaction") {
            updates.push({
                update_id: offset,
                message_reaction: {
                    message_id: offset - record.relativeReverseOffset!,
                    user: {
                        id: getUserId(record.character!),
                        username: record.character,
                        is_bot: record.character !== "Hamlet",
                    },
                    new_reaction: [
                        {
                            type: "emoji",
                            emoji: record.reaction,
                        } as components["schemas"]["ReactionTypeEmoji"]
                    ],
                },
            } as Req<components["schemas"]["Update"]>);
        } else {
            continue;
        }

        offset++;
    }
    
    return updates;
}