pragma solidity ^0.4.24;

import "./ERC20Token.sol";

interface token {
    function transfer(address receiver, uint amount) external;
}

contract Crowdsale is owned{
    address private beneficiary;
    uint public amountRaised;
    uint public price;
    token private tokenReward;
    uint private tokenAmount ;
    mapping(address => uint256) public balanceOf;
    bool private fundingGoalReached = false;
    bool private crowdsaleClosed = false;
    address private tokenAddress;

    event FundTransfer(address indexed backer, uint indexed amount);
    event FinishICO(address owner, uint amount);

    /**
     * Constructor function
     *
     * Setup the owner
     */
    constructor(
        uint tokensToSell,
        address ifSuccessfulSendTo,
        uint miliEtherCostOfEachToken,
        string tokenName,
        string tokenSymbol
    ) public {
        tokenAddress = new TokenERC20(tokensToSell,tokenName,tokenSymbol);
        tokenAmount = tokensToSell;
        beneficiary = ifSuccessfulSendTo;
        price = miliEtherCostOfEachToken * 0.001 ether;
        tokenReward = token(tokenAddress);
    }

    /**
     * Fallback function
     *
     * The function without name is the default function that is called whenever anyone sends funds to a contract
     */
    function () public payable {
        require(!crowdsaleClosed);
        uint amount = msg.value;
        balanceOf[msg.sender] += amount;
        amountRaised += amount;
        tokenReward.transfer(msg.sender, amount * 10**18 / price);
        emit FundTransfer(msg.sender, amount);
    }

    function finishSale() public onlyOwner{
        crowdsaleClosed = true;
        if (beneficiary.send(amountRaised)) {
                emit FinishICO(beneficiary, amountRaised);
        } 
    }
}