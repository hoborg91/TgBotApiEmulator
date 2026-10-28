export type Record = {
    type: "remark" | "speech" | "reaction",
    act: number,
    scene: number,
    content?: string,
    character?: string,
    speech?: string,
    reaction?: string,
    relativeReverseOffset?: number,
};

// https://core.telegram.org/bots/api#reactiontype
// "❤", "👍", "👎", "🔥", "🥰", "👏", "😁", "🤔", "🤯", "😱", "🤬", "😢", 
// "🎉", "🤩", "🤮", "💩", "🙏", "👌", "🕊", "🤡", "🥱", "🥴", "😍", "🐳", 
// "❤‍🔥", "🌚", "🌭", "💯", "🤣", "⚡", "🍌", "🏆", "💔", "🤨", "😐", "🍓", 
// "🍾", "💋", "🖕", "😈", "😴", "😭", "🤓", "👻", "👨‍💻", "👀", "🎃", "🙈", 
// "😇", "😨", "🤝", "✍", "🤗", "🫡", "🎅", "🎄", "☃", "💅", "🤪", "🗿", 
// "🆒", "💘", "🙉", "🦄", "😘", "💊", "🙊", "😎", "👾", "🤷‍♂", "🤷", "🤷‍♀", "😡"

export function Hamlet(): Record[] {
    return [
  {
    "type": "remark",
    "content": "Enter King Claudius, Queen Gertrude, Hamlet, Polonius, Laertes, Voltimand, Cornelius, and Lords",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Though yet of Hamlet our dear brother's death\nThe memory be green, and that it us befitted\nTo bear our hearts in grief and our whole kingdom\nTo be contracted in one brow of woe,\nYet so far hath discretion fought with nature\nThat we with wisest sorrow think on him,\nTogether with remembrance of ourselves.\nTherefore our sometime sister, now our queen,\nThe imperial jointress to this warlike state,\nHave we, as 'twere with a defeated joy,—\nWith an auspicious and a dropping eye,\nWith mirth in funeral and with dirge in marriage,\nIn equal scale weighing delight and dole,—\nTaken to wife: nor have we herein barr'd\nYour better wisdoms, which have freely gone\nWith this affair along. For all, our thanks.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Now follows, that you know, young Fortinbras,\nHolding a weak supposal of our worth,\nOr thinking by our late dear brother's death\nOur state to be disjoint and out of frame,\nColleagued with the dream of his advantage,\nHe hath not fail'd to pester us with message,\nImporting the surrender of those lands\nLost by his father, with all bonds of law,\nTo our most valiant brother. So much for him.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Now for ourself and for this time of meeting:\nThus much the business is: we have here writ\nTo Norway, uncle of young Fortinbras,—\nWho, impotent and bed-rid, scarcely hears\nOf this his nephew's purpose,—to suppress\nHis further gait herein; in that the levies,\nThe lists and full proportions, are all made\nOut of his subject: and we here dispatch\nYou, good Cornelius, and you, Voltimand,\nFor bearers of this greeting to old Norway;\nGiving to you no further personal power\nTo business with the king, more than the scope\nOf these delated articles allow.\nFarewell, and let your haste commend your duty.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Cornelius",
    "speech": "In that and all things will we show our duty.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Voltimand",
    "speech": "In that and all things will we show our duty.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "King Claudius",
    "reaction": "👍",
    "relativeReverseOffset": 2,
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "King Claudius",
    "reaction": "👍",
    "relativeReverseOffset": 2,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "We doubt it nothing: heartily farewell.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "remark",
    "content": "Exeunt Voltimand and Cornelius",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "And now, Laertes, what's the news with you?\nYou told us of some suit; what is't, Laertes?\nYou cannot speak of reason to the Dane,\nAnd loose your voice: what wouldst thou beg, Laertes,\nThat shall not be my offer, not thy asking?\nThe head is not more native to the heart,\nThe hand more instrumental to the mouth,\nThan is the throne of Denmark to thy father.\nWhat wouldst thou have, Laertes?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Laertes",
    "speech": "My dread lord,\nYour leave and favour to return to France;\nFrom whence though willingly I came to Denmark,\nTo show my duty in your coronation,\nYet now, I must confess, that duty done,\nMy thoughts and wishes bend again toward France\nAnd bow them to your gracious leave and pardon.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Have you your father's leave? What says Polonius?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Polonius",
    "speech": "He hath, my lord, wrung from me my slow leave\nBy laboursome petition, and at last\nUpon his will I seal'd my hard consent:\nI do beseech you, give him leave to go.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Take thy fair hour, Laertes; time be thine,\nAnd thy best graces spend it at thy will!\nBut now, my cousin Hamlet, and my son,—",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "Hamlet",
    "reaction": "😡",
    "relativeReverseOffset": 1,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "A little more than kin, and less than kind.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "How is it that the clouds still hang on you?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Not so, my lord; I am too much i' the sun.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Queen Gertrude",
    "speech": "Good Hamlet, cast thy nighted colour off,\nAnd let thine eye look like a friend on Denmark.\nDo not for ever with thy vailed lids\nSeek for thy noble father in the dust:\nThou know'st 'tis common; all that lives must die,\nPassing through nature to eternity.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Ay, madam, it is common.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Queen Gertrude",
    "speech": "If it be,\nWhy seems it so particular with thee?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Seems, madam! nay it is; I know not 'seems.'\n'Tis not alone my inky cloak, good mother,\nNor customary suits of solemn black,\nNor windy suspiration of forced breath,\nNo, nor the fruitful river in the eye,\nNor the dejected 'havior of the visage,\nTogether with all forms, moods, shapes of grief,\nThat can denote me truly: these indeed seem,\nFor they are actions that a man might play:\nBut I have that within which passeth show;\nThese but the trappings and the suits of woe.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "'Tis sweet and commendable in your nature, Hamlet,\nTo give these mourning duties to your father:\nBut, you must know, your father lost a father;\nThat father lost, lost his, and the survivor bound\nIn filial obligation for some term\nTo do obsequious sorrow: but to persever\nIn obstinate condolement is a course\nOf impious stubbornness; 'tis unmanly grief;\nIt shows a will most incorrect to heaven,\nA heart unfortified, a mind impatient,\nAn understanding simple and unschool'd:\nFor what we know must be and is as common\nAs any the most vulgar thing to sense,\nWhy should we in our peevish opposition\nTake it to heart? Fie! 'tis a fault to heaven,\nA fault against the dead, a fault to nature,\nTo reason most absurd: whose common theme\nIs death of fathers, and who still hath cried,\nFrom the first corse till he that died to-day,\n'This must be so.' We pray you, throw to earth\nThis unprevailing woe, and think of us\nAs of a father: for let the world take note,\nYou are the most immediate to our throne;\nAnd with no less nobility of love\nThan that which dearest father bears his son,\nDo I impart toward you. For your intent\nIn going back to school in Wittenberg,\nIt is most retrograde to our desire:\nAnd we beseech you, bend you to remain\nHere, in the cheer and comfort of our eye,\nOur chiefest courtier, cousin, and our son.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Queen Gertrude",
    "speech": "Let not thy mother lose her prayers, Hamlet:\nI pray thee, stay with us; go not to Wittenberg.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I shall in all my best obey you, madam.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "Hamlet",
    "reaction": "🫡",
    "relativeReverseOffset": 2,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "King Claudius",
    "speech": "Why, 'tis a loving and a fair reply:\nBe as ourself in Denmark. Madam, come;\nThis gentle and unforced accord of Hamlet\nSits smiling to my heart: in grace whereof,\nNo jocund health that Denmark drinks to-day,\nBut the great cannon to the clouds shall tell,\nAnd the king's rouse the heavens all bruit again,\nRe-speaking earthly thunder. Come away.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "remark",
    "content": "Exeunt all but Hamlet",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "O, that this too too solid flesh would melt\nThaw and resolve itself into a dew!\nOr that the Everlasting had not fix'd\nHis canon 'gainst self-slaughter! O God! God!\nHow weary, stale, flat and unprofitable,\nSeem to me all the uses of this world!\nFie on't! ah fie! 'tis an unweeded garden,\nThat grows to seed; things rank and gross in nature\nPossess it merely. That it should come to this!",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "But two months dead: nay, not so much, not two:\nSo excellent a king; that was, to this,\nHyperion to a satyr; so loving to my mother\nThat he might not beteem the winds of heaven\nVisit her face too roughly. Heaven and earth!",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Must I remember? why, she would hang on him,\nAs if increase of appetite had grown\nBy what it fed on: and yet, within a month—\nLet me not think on't—Frailty, thy name is woman!—\nA little month, or ere those shoes were old\nWith which she follow'd my poor father's body,\nLike Niobe, all tears:—why she, even she—\nO, God! a beast, that wants discourse of reason,\nWould have mourn'd longer—married with my uncle,\nMy father's brother, but no more like my father\nThan I to Hercules: within a month:\nEre yet the salt of most unrighteous tears\nHad left the flushing in her galled eyes,\nShe married. O, most wicked speed, to post\nWith such dexterity to incestuous sheets!\nIt is not nor it cannot come to good:\nBut break, my heart; for I must hold my tongue.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "remark",
    "content": "Enter Horatio, Marcellus, and Bernardo",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Hail to your lordship!",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "Hamlet",
    "reaction": "🤩",
    "relativeReverseOffset": 1,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I am glad to see you well:\nHoratio,—or I do forget myself.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "The same, my lord, and your poor servant ever.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Sir, my good friend; I'll change that name with you:\nAnd what make you from Wittenberg, Horatio? Marcellus?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "My good lord—",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I am very glad to see you. Good even, sir.\nBut what, in faith, make you from Wittenberg?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "A truant disposition, good my lord.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I would not hear your enemy say so,\nNor shall you do mine ear that violence,\nTo make it truster of your own report\nAgainst yourself: I know you are no truant.\nBut what is your affair in Elsinore?\nWe'll teach you to drink deep ere you depart.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "My lord, I came to see your father's funeral.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I pray thee, do not mock me, fellow-student;\nI think it was to see my mother's wedding.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Indeed, my lord, it follow'd hard upon.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Thrift, thrift, Horatio! the funeral baked meats\nDid coldly furnish forth the marriage tables.\nWould I had met my dearest foe in heaven\nOr ever I had seen that day, Horatio!\nMy father!—methinks I see my father.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Where, my lord?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "In my mind's eye, Horatio.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "I saw him once; he was a goodly king.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "He was a man, take him for all in all,\nI shall not look upon his like again.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "My lord, I think I saw him yesternight.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "Hamlet",
    "reaction": "🤔",
    "relativeReverseOffset": 1,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Saw? who?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "My lord, the king your father.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "The king my father!",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Season your admiration for a while\nWith an attent ear, till I may deliver,\nUpon the witness of these gentlemen,\nThis marvel to you.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "For God's love, let me hear.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Two nights together had these gentlemen,\nMarcellus and Bernardo, on their watch,\nIn the dead vast and middle of the night,\nBeen thus encounter'd. A figure like your father,\nArmed at point exactly, cap-a-pe,\nAppears before them, and with solemn march\nGoes slow and stately by them: thrice he walk'd\nBy their oppress'd and fear-surprised eyes,\nWithin his truncheon's length; whilst they, distilled\nAlmost to jelly with the act of fear,\nStand dumb and speak not to him. This to me\nIn dreadful secrecy impart they did;\nAnd I with them the third night kept the watch:\nWhere, as they had delivered, both in time,\nForm of the thing, each word made true and good,\nThe apparition comes: I knew your father;\nThese hands are not more like.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "But where was this?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "My lord, upon the platform where we watch'd.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Did you not speak to it?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "My lord, I did;\nBut answer made it none: yet once methought\nIt lifted up its head and did address\nItself to motion, like as it would speak;\nBut even then the morning cock crew loud,\nAnd at the sound it shrunk in haste away,\nAnd vanish'd from our sight.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "'Tis very strange.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "As I do live, my honour'd lord, 'tis true;\nAnd we did think it writ down in our duty\nTo let you know of it.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Indeed, indeed, sirs, but this troubles me.\nHold you the watch to-night?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "We do, my lord.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Bernardo",
    "speech": "We do, my lord.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Arm'd, say you?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "Arm'd, my lord.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Bernardo",
    "speech": "Arm'd, my lord.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "From top to toe?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "My lord, from head to foot.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Bernardo",
    "speech": "My lord, from head to foot.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Then saw you not his face?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "O, yes, my lord; he wore his beaver up.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "What, look'd he frowningly?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "A countenance more in sorrow than in anger.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Pale or red?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Nay, very pale.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "And fix'd his eyes upon you?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Most constantly.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I would I had been there.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "It would have much amazed you.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Very like, very like. Stay'd it long?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "While one with moderate haste might tell a hundred.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Marcellus",
    "speech": "Longer.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Bernardo",
    "speech": "Longer.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "Not when I saw't.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "His beard was grizzled—no?",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "It was, as I have seen it in his life,\nA sable silver'd.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "I will watch to-night;\nPerchance ’twill walk again.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "reaction",
    "character": "Horatio",
    "reaction": "💯",
    "relativeReverseOffset": 1,
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Horatio",
    "speech": "I'll warrant it will.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "If it assume my noble father's person,\nI'll speak to it, though hell itself should gape\nAnd bid me hold my peace. I pray you all,\nIf you have hitherto conceal'd this sight,\nLet it be tenable in your silence still;\nAnd whatsoever else shall hap to-night,\nGive it an understanding, but no tongue:\nI will requite your loves. So, fare you well:\nUpon the platform, 'twixt eleven and twelve,\nI'll visit you.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "ALL",
    "speech": "Our duty to your honour.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "Your loves, as mine to you: farewell.",
    "act": 1,
    "scene": 2
  },
  {
    "type": "remark",
    "content": "Exeunt all but Hamlet",
    "act": 1,
    "scene": 2
  },
  {
    "type": "speech",
    "character": "Hamlet",
    "speech": "My father's spirit in arms! all is not well;\nI doubt some foul play: would the night were come!\nTill then sit still, my soul: foul deeds will rise,\nThough all the earth o'erwhelm them, to men's eyes.",
    "act": 1,
    "scene": 2
  }
];
}